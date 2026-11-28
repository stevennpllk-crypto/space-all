package com.jassim.engine

/**
 * CPU-side indexed triangle mesh data.
 *
 * Positions, colors, and optional normals use tightly packed XYZ/RGB triples.
 * This class validates geometry before it is uploaded to the GPU. It does not
 * own OpenGL objects and can therefore be created safely outside the GL thread.
 */
class EngineMesh(
    positions: FloatArray,
    colors: FloatArray,
    indices: IntArray,
    normals: FloatArray? = null
) {
    private val positionData = positions.copyOf()
    private val colorData = colors.copyOf()
    private val indexData = indices.copyOf()
    private val normalData = normals?.copyOf()

    val positions: FloatArray
        get() = positionData.copyOf()

    val colors: FloatArray
        get() = colorData.copyOf()

    val indices: IntArray
        get() = indexData.copyOf()

    val normals: FloatArray?
        get() = normalData?.copyOf()

    val vertexCount: Int
        get() = positionData.size / COMPONENTS_PER_VERTEX

    val triangleCount: Int
        get() = indexData.size / INDICES_PER_TRIANGLE

    init {
        require(positionData.isNotEmpty() && positionData.size % COMPONENTS_PER_VERTEX == 0) {
            "Positions must contain one or more complete XYZ vertices"
        }
        require(colorData.size == positionData.size) {
            "Colors must contain one RGB triple for every vertex"
        }
        require(positionData.all(Float::isFinite)) { "Positions must be finite" }
        require(colorData.all(Float::isFinite)) { "Colors must be finite" }
        require(normalData == null || normalData.size == positionData.size) {
            "Normals must contain one XYZ triple for every vertex"
        }
        require(normalData == null || normalData.all(Float::isFinite)) {
            "Normals must be finite"
        }
        require(indexData.isNotEmpty() && indexData.size % INDICES_PER_TRIANGLE == 0) {
            "Indices must contain one or more complete triangles"
        }
        require(indexData.all { it in 0 until vertexCount }) {
            "Every index must reference an existing vertex"
        }
    }

    companion object {
        private const val COMPONENTS_PER_VERTEX = 3
        private const val INDICES_PER_TRIANGLE = 3
    }
}
