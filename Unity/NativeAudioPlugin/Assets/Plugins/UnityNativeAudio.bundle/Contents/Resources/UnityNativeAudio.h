//
//  NativeAudioPlugin.h
//  TestRtAudio
//
//  Created by asus4 on 2013/05/08.
//  Copyright (c) 2013 Koki Ibukuro. All rights reserved.
//

#pragma once

#include <iostream>
#include <CoreAudio/AudioHardware.h>
#include "RtAudio.h"

typedef signed short MY_TYPE;

#define _BUFFER_FRAMES 512
#define _SAMPLE_RATE 44100

// channels, bufferBytes, buffer
typedef void (*NativeAudioCallback)(unsigned int channels, unsigned int bufferBytes, MY_TYPE*buffer);

extern "C"
{
    int startNativeAudio(int deviceId, NativeAudioCallback callback); // return success or not
    void stopNativeAudio();
    
    const char* getAudioDeviceName(unsigned int deviceId);
    unsigned int getAudioDeviceCount();
    unsigned int getDefaultAudioDevice();

    unsigned int getAudioInputChannels(unsigned int deviceId);

    unsigned int getSampleRate();
    unsigned int getBufferFrames();

}
