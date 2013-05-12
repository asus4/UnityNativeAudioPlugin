//
//  main.cpp
//
//  Created by asus4 on 2013/05/08.
//  Copyright (c) 2013 Koki Ibukuro.
//

#include "UnityNativeAudio.h"
#include <iostream>

void callback(unsigned int channels, unsigned int b, signed short* buffer) {
    std::cout << " channels:" << channels << " b:" << b << std::endl;
    
    unsigned int i,j;
    for(i=0; i<512; i++) {
        for(j=0; j<channels; j++) {
            std::cout << "[" << j << "]" << *buffer++;
        }
        std::cout << std::endl;
    }
}

int mic_process() {
    char input;
    int error = startNativeAudio(0, 2, &callback);
    
    if(error == 0) {
        std::cout << "Success to Start." << std::endl;
    }
    else {
        return 1;
    }
    
    std::cout << "Playing again ... press <enter> to close the stream.\n";
    std::cin.get(input);
    
    stopNativeAudio();
    std::cout << "\nSuccess to Quit." << std::endl;
    
    return 0;
}

int main(int argc, const char * argv[])
{
 //   if(mic_process() != 0) return 1;
//    if(mic_process() != 0) return 1;

    std::cout << "\nbufferframes:" << getBufferFrames() << std::endl;
    std::cout << "\nchannels:" << getAudioInputChannels(0) << std::endl;
    
    MY_TYPE *buffer = (MY_TYPE*)malloc(_BUFFER_FRAMES * 2 * sizeof(MY_TYPE));
    
    //char input;
    int i=0;
    int error = startNativeAudio(0, 2, &callback);
    if(error) {
        std::cout << "error start" << std::endl;
        return 1;
    }
    
    struct timespec treq, trem;
    treq.tv_sec = 0;
    treq.tv_nsec = 10 * 1000000 ; // 10 mill sec
    
    while (i < 200) { // 2sec
        nanosleep(&treq, &trem);
        while(getAudioBuffer(buffer) == 0) {
            std::cout <<  i << ":" << buffer << std::endl;
        }
        callback(2, 512, buffer);
        i++;
    }
    stopNativeAudio();
    
    std::cout << "finished:" << i << std::endl;
    
    
    return 0;
}

