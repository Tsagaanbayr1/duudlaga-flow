import Foundation

struct WAVEncoder {
    static func encode(samples: [Float], sampleRate: Int = 16000, channels: Int = 1, bitDepth: Int = 16) -> Data {
        let int16Samples = samples.map { sample -> Int16 in
            let clamped = max(-1.0, min(1.0, sample))
            return Int16(clamped * Float(Int16.max))
        }

        let dataSize = int16Samples.count * MemoryLayout<Int16>.size
        let headerSize = 44
        let fileSize = headerSize + dataSize

        var data = Data(capacity: fileSize)

        // RIFF header
        data.append(contentsOf: "RIFF".utf8)
        data.append(littleEndian: UInt32(fileSize - 8))
        data.append(contentsOf: "WAVE".utf8)

        // fmt subchunk
        data.append(contentsOf: "fmt ".utf8)
        data.append(littleEndian: UInt32(16)) // subchunk size
        data.append(littleEndian: UInt16(1))  // PCM format
        data.append(littleEndian: UInt16(channels))
        data.append(littleEndian: UInt32(sampleRate))
        let byteRate = sampleRate * channels * (bitDepth / 8)
        data.append(littleEndian: UInt32(byteRate))
        let blockAlign = channels * (bitDepth / 8)
        data.append(littleEndian: UInt16(blockAlign))
        data.append(littleEndian: UInt16(bitDepth))

        // data subchunk
        data.append(contentsOf: "data".utf8)
        data.append(littleEndian: UInt32(dataSize))

        for sample in int16Samples {
            data.append(littleEndian: sample)
        }

        return data
    }
}

private extension Data {
    mutating func append(littleEndian value: UInt32) {
        var v = value.littleEndian
        append(Data(bytes: &v, count: MemoryLayout<UInt32>.size))
    }

    mutating func append(littleEndian value: UInt16) {
        var v = value.littleEndian
        append(Data(bytes: &v, count: MemoryLayout<UInt16>.size))
    }

    mutating func append(littleEndian value: Int16) {
        var v = value.littleEndian
        append(Data(bytes: &v, count: MemoryLayout<Int16>.size))
    }
}
