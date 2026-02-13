import Foundation

enum ChimegeAPIError: LocalizedError {
    case invalidAudio(String)
    case invalidToken(String)
    case serverError(String)
    case overloaded(String)
    case emptyResponse
    case networkError(Error)
    case unexpectedStatus(Int, String)
    case transcriptionTimeout

    var errorDescription: String? {
        switch self {
        case .invalidAudio(let body): return "Аудио алдаа: \(body)"
        case .invalidToken(let body): return "Token алдаа: \(body)"
        case .serverError(let body): return "Серверийн алдаа: \(body)"
        case .overloaded: return "Сервер ачаалалтай, дахин оролдоно уу"
        case .emptyResponse: return "Хоосон хариу"
        case .networkError(let error): return "Сүлжээний алдаа: \(error.localizedDescription)"
        case .unexpectedStatus(let code, let body): return "Алдаа \(code): \(body)"
        case .transcriptionTimeout: return "Хугацаа дууссан, дахин оролдоно уу"
        }
    }
}

private struct SttLongSubmitResponse: Decodable {
    let uuid: String
    let duration: Double
}

private struct SttLongTranscriptResponse: Decodable {
    let done: Bool
    let transcription: String?
    let duration: Double?
}

final class ChimegeAPIClient {
    private let session: URLSession
    private let sttLongURL = URL(string: "https://api.chimege.com/v1.2/stt-long")!
    private let sttLongTranscriptURL = URL(string: "https://api.chimege.com/v1.2/stt-long-transcript")!

    init() {
        let config = URLSessionConfiguration.default
        config.timeoutIntervalForRequest = 30
        self.session = URLSession(configuration: config)
    }

    func transcribe(audioData: Data, token: String, punctuate: Bool) async throws -> String {
        let cleanToken = token.trimmingCharacters(in: .whitespacesAndNewlines)

        // Step 1: Submit audio
        let uuid = try await submitAudio(audioData: audioData, token: cleanToken)

        // Step 2: Poll for result
        let text = try await pollForTranscript(uuid: uuid, token: cleanToken)
        return text
    }

    private func submitAudio(audioData: Data, token: String) async throws -> String {
        var request = URLRequest(url: sttLongURL)
        request.httpMethod = "POST"
        request.setValue(token, forHTTPHeaderField: "Token")
        request.setValue("application/octet-stream", forHTTPHeaderField: "Content-Type")
        request.httpBody = audioData

        let data: Data
        let response: URLResponse

        do {
            (data, response) = try await session.data(for: request)
        } catch {
            throw ChimegeAPIError.networkError(error)
        }

        guard let httpResponse = response as? HTTPURLResponse else {
            throw ChimegeAPIError.serverError("Invalid response")
        }

        let body = String(data: data, encoding: .utf8) ?? "(empty)"

        switch httpResponse.statusCode {
        case 200:
            let submitResponse = try JSONDecoder().decode(SttLongSubmitResponse.self, from: data)
            return submitResponse.uuid
        case 400:
            throw ChimegeAPIError.invalidAudio(body)
        case 403:
            throw ChimegeAPIError.invalidToken(body)
        case 500:
            throw ChimegeAPIError.serverError(body)
        case 503:
            throw ChimegeAPIError.overloaded(body)
        default:
            throw ChimegeAPIError.unexpectedStatus(httpResponse.statusCode, body)
        }
    }

    private func pollForTranscript(uuid: String, token: String) async throws -> String {
        let maxAttempts = 60 // up to 60 seconds
        for _ in 0..<maxAttempts {
            var request = URLRequest(url: sttLongTranscriptURL)
            request.httpMethod = "GET"
            request.setValue(token, forHTTPHeaderField: "Token")
            request.setValue(uuid, forHTTPHeaderField: "UUID")

            let (data, response) = try await session.data(for: request)

            guard let httpResponse = response as? HTTPURLResponse else { continue }

            if httpResponse.statusCode == 200 {
                let result = try JSONDecoder().decode(SttLongTranscriptResponse.self, from: data)
                if result.done {
                    let text = result.transcription?.trimmingCharacters(in: .whitespacesAndNewlines) ?? ""
                    if text.isEmpty {
                        throw ChimegeAPIError.emptyResponse
                    }
                    return text
                }
            } else {
                let body = String(data: data, encoding: .utf8) ?? ""
                if httpResponse.statusCode == 403 {
                    throw ChimegeAPIError.invalidToken(body)
                }
            }

            try await Task.sleep(for: .seconds(1))
        }

        throw ChimegeAPIError.transcriptionTimeout
    }

    /// Test the API token by sending a minimal WAV
    func testToken(_ token: String) async -> Result<String, ChimegeAPIError> {
        let cleanToken = token.trimmingCharacters(in: .whitespacesAndNewlines)

        // Generate a tiny 0.5s silent WAV
        let silentSamples = [Float](repeating: 0.001, count: 8000)
        let wavData = WAVEncoder.encode(samples: silentSamples, sampleRate: 16000, channels: 1, bitDepth: 16)

        do {
            let uuid = try await submitAudio(audioData: wavData, token: cleanToken)
            return .success("Token зөв! (UUID: \(uuid.prefix(8))...)")
        } catch let error as ChimegeAPIError {
            return .failure(error)
        } catch {
            return .failure(.networkError(error))
        }
    }
}
