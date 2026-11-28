package com.jassim.engine

import org.junit.Assert.assertEquals
import org.junit.Assert.assertNotSame
import org.junit.Assert.assertNull
import org.junit.Test

class EngineMeshTest {
    private val trianglePositions = floatArrayOf(
        0.0f, 0.0f, 0.0f,
        1.0f, 0.0f, 0.0f,
        0.0f, 1.0f, 0.0f
    )
    private val triangleColors = floatArrayOf(
        1.0f, 0.0f, 0.0f,
        0.0f, 1.0f, 0.0f,
        0.0f, 0.0f, 1.0f
    )
    private val triangleIndices = intArrayOf(0, 1, 2)

    @Test
    fun acceptsValidIndexedTriangle() {
        val mesh = EngineMesh(trianglePositions, triangleColors, triangleIndices)
        assertEquals(3, mesh.vertexCount)
        assertEquals(1, mesh.triangleCount)
        assertNull(mesh.normals)
    }

    @Test
    fun acceptsOptionalPerVertexNormals() {
        val normals = FloatArray(9) { 1.0f }
        val mesh = EngineMesh(trianglePositions, triangleColors, triangleIndices, normals)
        assertEquals(9, mesh.normals?.size)
    }

    @Test(expected = IllegalArgumentException::class)
    fun rejectsIncompletePositionTriples() {
        EngineMesh(floatArrayOf(0.0f, 1.0f), triangleColors, triangleIndices)
    }

    @Test(expected = IllegalArgumentException::class)
    fun rejectsColorCountThatDoesNotMatchVertices() {
        EngineMesh(trianglePositions, floatArrayOf(1.0f, 0.0f, 0.0f), triangleIndices)
    }

    @Test(expected = IllegalArgumentException::class)
    fun rejectsIncompleteTriangleIndices() {
        EngineMesh(trianglePositions, triangleColors, intArrayOf(0, 1))
    }

    @Test(expected = IllegalArgumentException::class)
    fun rejectsIndexOutsideVertexRange() {
        EngineMesh(trianglePositions, triangleColors, intArrayOf(0, 1, 3))
    }

    @Test(expected = IllegalArgumentException::class)
    fun rejectsNonFiniteVertexData() {
        val invalidPositions = trianglePositions.copyOf().apply { this[0] = Float.NaN }
        EngineMesh(invalidPositions, triangleColors, triangleIndices)
    }

    @Test(expected = IllegalArgumentException::class)
    fun rejectsNormalsWithWrongComponentCount() {
        EngineMesh(trianglePositions, triangleColors, triangleIndices, floatArrayOf(0.0f, 1.0f, 0.0f))
    }

    @Test
    fun copiesInputAndReturnedArrays() {
        val sourcePositions = trianglePositions.copyOf()
        val mesh = EngineMesh(sourcePositions, triangleColors, triangleIndices)
        sourcePositions[0] = 99.0f
        val exposedPositions = mesh.positions
        exposedPositions[0] = 88.0f
        assertEquals(0.0f, mesh.positions[0], 0.0001f)
        assertNotSame(sourcePositions, mesh.positions)
    }
}
