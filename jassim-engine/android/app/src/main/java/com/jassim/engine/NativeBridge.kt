package com.jassim.engine

object NativeBridge {
    init {
        System.loadLibrary("jassim_engine_native")
    }

    external fun nativeEngineVersion(): String
    external fun nativeCreateRootNode(): Int
}
