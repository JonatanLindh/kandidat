#[compute]
#version 450

struct Triangle{
    vec4 p1;
    vec4 p2;
    vec4 p3;
};

const int cornerIndexAFromEdge[12] = {0, 1, 2, 3, 4, 5, 6, 7, 0, 1, 2, 3};
const int cornerIndexBFromEdge[12] = {1, 2, 3, 0, 5, 6, 7, 4, 4, 5, 6, 7};

const int offsets[256] = {0, 0, 3, 6, 12, 15, 21, 27, 36, 39, 45, 51, 60, 66, 75, 84, 90, 93, 99, 105, 114, 120, 129, 138, 150, 156, 165, 174, 186, 195, 207, 219, 228, 231, 237, 243, 252, 258, 267, 276, 288, 294, 303, 312, 324, 333, 345, 357, 366, 372, 381, 390, 396, 405, 417, 429, 438, 447, 459, 471, 480, 492, 507, 522, 528, 531, 537, 543, 552, 558, 567, 576, 588, 594, 603, 612, 624, 633, 645, 657, 666, 672, 681, 690, 702, 711, 723, 735, 750, 759, 771, 783, 798, 810, 825, 840, 852, 858, 867, 876, 888, 897, 909, 915, 924, 933, 945, 957, 972, 984, 999, 1008, 1014, 1023, 1035, 1047, 1056, 1068, 1083, 1092, 1098, 1110, 1125, 1140, 1152, 1167, 1173, 1185, 1188, 1191, 1197, 1203, 1212, 1218, 1227, 1236, 1248, 1254, 1263, 1272, 1284, 1293, 1305, 1317, 1326, 1332, 1341, 1350, 1362, 1371, 1383, 1395, 1410, 1419, 1425, 1437, 1446, 1458, 1467, 1482, 1488, 1494, 1503, 1512, 1524, 1533, 1545, 1557, 1572, 1581, 1593, 1605, 1620, 1632, 1647, 1662, 1674, 1683, 1695, 1707, 1716, 1728, 1743, 1758, 1770, 1782, 1791, 1806, 1812, 1827, 1839, 1845, 1848, 1854, 1863, 1872, 1884, 1893, 1905, 1917, 1932, 1941, 1953, 1965, 1980, 1986, 1995, 2004, 2010, 2019, 2031, 2043, 2058, 2070, 2085, 2100, 2106, 2118, 2127, 2142, 2154, 2163, 2169, 2181, 2184, 2193, 2205, 2217, 2232, 2244, 2259, 2268, 2280, 2292, 2307, 2322, 2328, 2337, 2349, 2355, 2358, 2364, 2373, 2382, 2388, 2397, 2409, 2415, 2418, 2427, 2433, 2445, 2448, 2454, 2457, 2460};
const int lengths[256] = {0, 3, 3, 6, 3, 6, 6, 9, 3, 6, 6, 9, 6, 9, 9, 6, 3, 6, 6, 9, 6, 9, 9, 12, 6, 9, 9, 12, 9, 12, 12, 9, 3, 6, 6, 9, 6, 9, 9, 12, 6, 9, 9, 12, 9, 12, 12, 9, 6, 9, 9, 6, 9, 12, 12, 9, 9, 12, 12, 9, 12, 15, 15, 6, 3, 6, 6, 9, 6, 9, 9, 12, 6, 9, 9, 12, 9, 12, 12, 9, 6, 9, 9, 12, 9, 12, 12, 15, 9, 12, 12, 15, 12, 15, 15, 12, 6, 9, 9, 12, 9, 12, 6, 9, 9, 12, 12, 15, 12, 15, 9, 6, 9, 12, 12, 9, 12, 15, 9, 6, 12, 15, 15, 12, 15, 6, 12, 3, 3, 6, 6, 9, 6, 9, 9, 12, 6, 9, 9, 12, 9, 12, 12, 9, 6, 9, 9, 12, 9, 12, 12, 15, 9, 6, 12, 9, 12, 9, 15, 6, 6, 9, 9, 12, 9, 12, 12, 15, 9, 12, 12, 15, 12, 15, 15, 12, 9, 12, 12, 9, 12, 15, 15, 12, 12, 9, 15, 6, 15, 12, 6, 3, 6, 9, 9, 12, 9, 12, 12, 15, 9, 12, 12, 15, 6, 9, 9, 6, 9, 12, 12, 15, 12, 15, 15, 6, 12, 9, 15, 12, 9, 6, 12, 3, 9, 12, 12, 15, 12, 15, 9, 12, 12, 15, 15, 6, 9, 12, 6, 3, 6, 9, 9, 6, 9, 12, 6, 3, 9, 6, 12, 3, 6, 3, 3, 0};

const ivec2[12] EDGE_TABLE = {
ivec2(0, 1),
ivec2(1, 2),
ivec2(2, 3),
ivec2(3, 0),

ivec2(4, 5),
ivec2(5, 6),
ivec2(6, 7),
ivec2(7, 4),

ivec2(0, 4),
ivec2(1, 5),
ivec2(2, 6),
ivec2(3, 7)
};

const ivec3[8] POINTS_TABLE = {
ivec3(0, 0, 0),
ivec3(0, 0, 1),
ivec3(1, 0, 1),
ivec3(1, 0, 0),

ivec3(0, 1, 0),
ivec3(0, 1, 1),
ivec3(1, 1, 1),
ivec3(1, 1, 0)
};

layout(set = 0, binding = 0, std430) restrict buffer TriangleBuffer {
    Triangle triangles[];
}
triangleBuffer;

layout(set = 0, binding = 1, std430) buffer DataPointsBuffer {
    float dataPoints[];
}
dataPointsBuffer;

layout(set = 0, binding = 2) uniform Uniforms {
    float sizeX;
    float sizeY;
    float sizeZ;
    float isoLevel;
    float scale;
    float xOffset;
    float yOffset;
    float zOffset;
} uniforms;

layout(set = 0, binding = 3) coherent buffer Counter{
    uint counter;
};

layout(set = 0, binding = 4) buffer TriangleTableBuffer{
    int triangulations[];
} triangleTable;


uint index(int x, int y, int z){
    return x + y * uint(uniforms.sizeX) + z * uint(uniforms.sizeY) * uint(uniforms.sizeZ);
}

vec3 evaluateTriangle(int tri, vec3 pos){
    ivec2 pointsIndices = EDGE_TABLE[tri];

    vec3 p0 = POINTS_TABLE[pointsIndices.x];
    vec3 p1 = POINTS_TABLE[pointsIndices.y];

    vec3 v0 = pos + p0;
    vec3 v1 = pos + p1;
    return mix(v0, v1, 0.5);
}

float calculateDensityLevel(vec3 pos){
    if(distance(vec3(32), pos) < 32) return 1;
    return 0;
}

layout(local_size_x = 8, local_size_y = 8, local_size_z = 8) in;
void main(){
    vec3 pos = gl_GlobalInvocationID.xyz;
    vec3 posOffset = vec3(uniforms.xOffset, uniforms.yOffset, uniforms.zOffset);
    if (pos.x >= uniforms.sizeX - 1 || pos.y >= uniforms.sizeY - 1 || pos.z >= uniforms.sizeZ - 1) return;
    
    int cubeIndex = 0;
    float isoLevel = uniforms.isoLevel;
    if (dataPointsBuffer.dataPoints[index(int(pos.x), int(pos.y), int(pos.z))] >= isoLevel) cubeIndex |= 1;
    if (dataPointsBuffer.dataPoints[index(int(pos.x), int(pos.y), int(pos.z + 1))] >= isoLevel) cubeIndex |= 2;
    if (dataPointsBuffer.dataPoints[index(int(pos.x + 1), int(pos.y), int(pos.z + 1))] >= isoLevel) cubeIndex |= 4;
    if (dataPointsBuffer.dataPoints[index(int(pos.x + 1), int(pos.y), int(pos.z))] >= isoLevel) cubeIndex |= 8;
  
    if (dataPointsBuffer.dataPoints[index(int(pos.x), int(pos.y + 1), int(pos.z))] >= isoLevel) cubeIndex |= 16;
    if (dataPointsBuffer.dataPoints[index(int(pos.x), int(pos.y + 1), int(pos.z + 1))] >= isoLevel) cubeIndex |= 32;
    if (dataPointsBuffer.dataPoints[index(int(pos.x + 1), int(pos.y + 1), int(pos.z + 1))] >= isoLevel) cubeIndex |= 64;
    if (dataPointsBuffer.dataPoints[index(int(pos.x + 1), int(pos.y + 1), int(pos.z))] >= isoLevel) cubeIndex |= 128;
    
    int numIndicies = lengths[cubeIndex];
    int offset = offsets[cubeIndex];
    for(int i = 0; i < numIndicies; i += 3){
        int tri = triangleTable.triangulations[offset + i];
        int tri2 = triangleTable.triangulations[offset + i + 1];
        int tri3 = triangleTable.triangulations[offset + i + 2];

        Triangle triangle;
        triangle.p1 = vec4(evaluateTriangle(tri, pos + posOffset), 0);
        triangle.p2 = vec4(evaluateTriangle(tri2, pos + posOffset), 0);
        triangle.p3 = vec4(evaluateTriangle(tri3, pos + posOffset), 0);

        uint index = atomicAdd(counter, 1u);
        triangleBuffer.triangles[index] = triangle;
    }
}