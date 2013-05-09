//
//  NativeAudioPlugin.cpp
//
//  Created by asus4 on 2013/05/08.
//  Copyright (c) 2013 Koki Ibukuro.
//

#include "UnityNativeAudio.h"

#define RT_FORMAT RTAUDIO_SINT16

struct UserData {
    unsigned long bufferBytes;
    unsigned int channels;
};

RtAudio *audio = 0;
UserData data;
NativeAudioCallback callbackFunc;

int input(void *outputBuffer, void *inputBuffer, unsigned int nBufferFrames,
          double streamTime, RtAudioStreamStatus status, void *data) {
    
    UserData *iData = (UserData *) data;    
    MY_TYPE *buffer = (MY_TYPE*) inputBuffer;

    callbackFunc(iData->channels, (unsigned int)iData->bufferBytes, buffer);
    return 0;
}


// サポートしているデバイスリストを表示
void listDevices() {
    RtAudio audio;
    
    unsigned int devices = audio.getDeviceCount();
    RtAudio::DeviceInfo info;
    
    for(unsigned int i=0; i<devices; i++) {
        info = audio.getDeviceInfo(i);
        
        std::cout << "============================" << std::endl;
        std::cout << "\nDevide ID:" << i << std::endl;
        std::cout << "Name:" << info.name << std::endl;
        if ( info.probed == false )
            std::cout << "Probe Status = UNsuccessful\n";
        else {
            std::cout << "Probe Status = Successful\n";
            std::cout << "Output Channels = " << info.outputChannels << '\n';
            std::cout << "Input Channels = " << info.inputChannels << '\n';
            std::cout << "Duplex Channels = " << info.duplexChannels << '\n';
            if ( info.isDefaultOutput ) {
                std::cout << "This is the default output device.\n";
            } else {
                std::cout << "This is NOT the default output device.\n";
            }
            if ( info.isDefaultInput ) { std::cout << "This is the default input device.\n";
            } else {
                std::cout << "This is NOT the default input device.\n";
            }
            if ( info.nativeFormats == 0 ) {
                std::cout << "No natively supported data formats(?)!";
            } else {
                std::cout << "Natively supported data formats:\n";
                if ( info.nativeFormats & RTAUDIO_SINT8 )
                    std::cout << "  8-bit int\n";
                if ( info.nativeFormats & RTAUDIO_SINT16 )
                    std::cout << "  16-bit int\n";
                if ( info.nativeFormats & RTAUDIO_SINT24 )
                    std::cout << "  24-bit int\n";
                if ( info.nativeFormats & RTAUDIO_SINT32 )
                    std::cout << "  32-bit int\n";
                if ( info.nativeFormats & RTAUDIO_FLOAT32 )
                    std::cout << "  32-bit float\n";
                if ( info.nativeFormats & RTAUDIO_FLOAT64 )
                    std::cout << "  64-bit float\n";
            }
            if ( info.sampleRates.size() < 1 ) {
                std::cout << "No supported sample rates found!";
            } else {
                std::cout << "Supported sample rates = ";
                for (unsigned int j=0; j<info.sampleRates.size(); j++)
                    std::cout << info.sampleRates[j] << " ";
            }
            std::cout << std::endl;
        }
    }
}



int startNativeAudio(int inDeviceId, NativeAudioCallback callback) {
    if(getAudioDeviceCount() < 1) {
        return -1; // has error
    }
    // stop before
    stopNativeAudio();
    
    // set call back
    callbackFunc = callback;
    
    // create new
    audio = new RtAudio();
    std::cout << "RtAudio Version " << RtAudio::getVersion() << std::endl;
    
    if(inDeviceId<0) {
        inDeviceId = audio->getDefaultInputDevice();
    }
    
    RtAudio::DeviceInfo deviceInfo = audio->getDeviceInfo(inDeviceId);
    
    unsigned int bufferFrames = _BUFFER_FRAMES;
    unsigned int sampleRate = _SAMPLE_RATE;
    unsigned int channels = deviceInfo.inputChannels;
    
    // make input parameters
    RtAudio::StreamParameters iParams;
    iParams.deviceId = inDeviceId;
    iParams.nChannels = channels;
    iParams.firstChannel = 0;
    
    // stream options
    RtAudio::StreamOptions options;
//    options.flags |= RTAUDIO_NONINTERLEAVED;
    
    try {
        audio->openStream(NULL, &iParams, RT_FORMAT, sampleRate, &bufferFrames, &input, (void*)&data, &options);
    }
    catch (RtError& e) {
        std::cout << e.getMessage() << std::endl;
        stopNativeAudio();
        return 1;
    }
    
    data.bufferBytes = bufferFrames * channels * sizeof(MY_TYPE);
    data.channels = channels;
    
    try {
        audio->startStream();
    }
    catch (RtError& e) {
        std::cout << e.getMessage() << std::endl;
        stopNativeAudio();
    }
    
    return 0; // success
}

void stopNativeAudio() {
    if(audio == 0) {
        return;
    }
    if(audio->isStreamOpen()) {
        audio->closeStream();
    }
    delete audio;
    audio = 0;
}

const char* getAudioDeviceName(unsigned int deviceId) {
    RtAudio audio;
    std::string name = audio.getDeviceInfo(deviceId).name;
    unsigned long len = name.length();
    char* c = new char[len+1];
    memcpy(c, name.c_str(), len+1);
    return c;
}

unsigned int getAudioDeviceCount() {
    RtAudio audio;
    return audio.getDeviceCount();
}

unsigned int getDefaultAudioDevice() {
    RtAudio audio;
    return audio.getDefaultInputDevice();
}

unsigned int getAudioInputChannels(unsigned int deviceId) {
    RtAudio audio;
    return audio.getDeviceInfo(deviceId).inputChannels;
}

unsigned int getBufferFrames() {
    return _BUFFER_FRAMES;
}

unsigned int getSampleRate() {
    return _SAMPLE_RATE;
}