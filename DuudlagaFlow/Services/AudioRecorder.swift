import AVFoundation
import Observation

@Observable
final class AudioRecorder {
    private var audioEngine: AVAudioEngine?
    private var accumulatedSamples: [Float] = []
    private(set) var isRecording = false
    var currentRMSLevel: Float = 0.0

    var availableMicrophones: [AVCaptureDevice] {
        AVCaptureDevice.DiscoverySession(
            deviceTypes: [.microphone],
            mediaType: .audio,
            position: .unspecified
        ).devices
    }

    func startRecording(deviceID: String? = nil) throws {
        let engine = AVAudioEngine()

        if let deviceID = deviceID, let audioDeviceID = deviceIDToAudioDeviceID(deviceID) {
            setInputDevice(audioDeviceID, on: engine)
        }

        let inputNode = engine.inputNode
        let nativeFormat = inputNode.outputFormat(forBus: 0)

        accumulatedSamples = []
        isRecording = true

        inputNode.installTap(onBus: 0, bufferSize: 1024, format: nativeFormat) { [weak self] buffer, _ in
            guard let self = self else { return }
            let channelData = buffer.floatChannelData?[0]
            let frameLength = Int(buffer.frameLength)

            guard let data = channelData, frameLength > 0 else { return }

            let samples = Array(UnsafeBufferPointer(start: data, count: frameLength))

            // Compute RMS
            var sumOfSquares: Float = 0
            for sample in samples {
                sumOfSquares += sample * sample
            }
            let rms = sqrt(sumOfSquares / Float(frameLength))

            DispatchQueue.main.async {
                self.accumulatedSamples.append(contentsOf: samples)
                self.currentRMSLevel = rms
            }
        }

        engine.prepare()
        try engine.start()
        self.audioEngine = engine
    }

    func stopRecording() -> Data? {
        guard let engine = audioEngine, isRecording else { return nil }

        engine.inputNode.removeTap(onBus: 0)
        engine.stop()
        isRecording = false

        let nativeFormat = engine.inputNode.outputFormat(forBus: 0)
        let nativeSampleRate = nativeFormat.sampleRate

        // Convert to 16kHz mono
        let targetSampleRate = Constants.Audio.sampleRate
        let converted: [Float]

        if nativeSampleRate != targetSampleRate {
            converted = resample(samples: accumulatedSamples, from: nativeSampleRate, to: targetSampleRate)
        } else {
            converted = accumulatedSamples
        }

        accumulatedSamples = []
        audioEngine = nil
        currentRMSLevel = 0

        guard !converted.isEmpty else { return nil }

        return WAVEncoder.encode(
            samples: converted,
            sampleRate: Int(targetSampleRate),
            channels: 1,
            bitDepth: 16
        )
    }

    private func resample(samples: [Float], from sourceSampleRate: Double, to targetSampleRate: Double) -> [Float] {
        let ratio = targetSampleRate / sourceSampleRate
        let outputLength = Int(Double(samples.count) * ratio)
        var output = [Float](repeating: 0, count: outputLength)

        for i in 0..<outputLength {
            let srcIndex = Double(i) / ratio
            let index0 = Int(srcIndex)
            let fraction = Float(srcIndex - Double(index0))

            if index0 + 1 < samples.count {
                output[i] = samples[index0] * (1 - fraction) + samples[index0 + 1] * fraction
            } else if index0 < samples.count {
                output[i] = samples[index0]
            }
        }

        return output
    }

    private func deviceIDToAudioDeviceID(_ uniqueID: String) -> AudioDeviceID? {
        var address = AudioObjectPropertyAddress(
            mSelector: kAudioHardwarePropertyDevices,
            mScope: kAudioObjectPropertyScopeGlobal,
            mElement: kAudioObjectPropertyElementMain
        )

        var dataSize: UInt32 = 0
        AudioObjectGetPropertyDataSize(AudioObjectID(kAudioObjectSystemObject), &address, 0, nil, &dataSize)

        let deviceCount = Int(dataSize) / MemoryLayout<AudioDeviceID>.size
        var devices = [AudioDeviceID](repeating: 0, count: deviceCount)
        AudioObjectGetPropertyData(AudioObjectID(kAudioObjectSystemObject), &address, 0, nil, &dataSize, &devices)

        for device in devices {
            var uidAddress = AudioObjectPropertyAddress(
                mSelector: kAudioDevicePropertyDeviceUID,
                mScope: kAudioObjectPropertyScopeGlobal,
                mElement: kAudioObjectPropertyElementMain
            )
            var uid: CFString = "" as CFString
            var uidSize = UInt32(MemoryLayout<CFString>.size)
            AudioObjectGetPropertyData(device, &uidAddress, 0, nil, &uidSize, &uid)

            if uid as String == uniqueID {
                return device
            }
        }
        return nil
    }

    private func setInputDevice(_ deviceID: AudioDeviceID, on engine: AVAudioEngine) {
        let audioUnit = engine.inputNode.audioUnit!
        var deviceIDValue = deviceID
        AudioUnitSetProperty(
            audioUnit,
            kAudioOutputUnitProperty_CurrentDevice,
            kAudioUnitScope_Global,
            0,
            &deviceIDValue,
            UInt32(MemoryLayout<AudioDeviceID>.size)
        )
    }
}
