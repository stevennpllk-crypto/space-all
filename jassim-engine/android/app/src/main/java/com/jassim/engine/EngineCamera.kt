package com.jassim.engine

/**
 * Thread-safe camera state shared by the UI thread and the GL render thread.
 * Angles are expressed in degrees; distance is the camera's Z offset.
 */
class EngineCamera {
    @Volatile var rotationX: Float = -18.0f
        private set
    @Volatile var rotationY: Float = 0.0f
        private set
    @Volatile var distance: Float = 2.8f
        private set

    @Synchronized
    fun rotateBy(deltaX: Float, deltaY: Float) {
        if (!deltaX.isFinite() || !deltaY.isFinite()) return
        rotationY = ((rotationY + deltaX) % 360.0f + 360.0f) % 360.0f
        rotationX = (rotationX + deltaY).coerceIn(-85.0f, 85.0f)
    }

    @Synchronized
    fun zoomBy(scaleFactor: Float) {
        if (!scaleFactor.isFinite() || scaleFactor <= 0.0f) return
        distance = (distance / scaleFactor).coerceIn(MIN_DISTANCE, MAX_DISTANCE)
    }

    companion object {
        const val MIN_DISTANCE = 1.7f
        const val MAX_DISTANCE = 6.0f
    }
}
