shader_type spatial;

// Normal Mapping for a Triplanar Shader - Based on Ben Golus 2017 Unity shader
// https://bgolus.medium.com/normal-mapping-for-a-triplanar-shader-10bf39dca05a

// Properties
uniform sampler2D albedo_texture : source_color;
uniform sampler2D normal_texture : hint_normal;
uniform sampler2D occlusion_texture : hint_default_white;
uniform float smoothness : hint_range(0.0, 1.0) = 0.5;
uniform float metallic : hint_range(0.0, 1.0) = 0.0;
uniform float occlusion_strength : hint_range(0.0, 1.0) = 1.0;
uniform vec2 texture_scale = vec2(1.0, 1.0);
uniform vec2 texture_offset = vec2(0.0, 0.0);


// Uncomment to offset UVs to prevent obvious mirroring
#define TRIPLANAR_UV_OFFSET

// Reoriented Normal Mapping
// http://blog.selfshadow.com/publications/blending-in-detail/
// Altered to take normals (-1 to 1 ranges) rather than unsigned normal maps (0 to 1 ranges)
vec3 blend_rnm(vec3 n1, vec3 n2) {
    n1.z += 1.0;
    n2.xy = -n2.xy;
    
    return n1 * dot(n1, n2) / n1.z - n2;
}

void fragment() {
    // Get world normal for triplanar projection
    vec3 world_normal = (INV_VIEW_MATRIX * vec4(NORMAL, 0.0)).xyz;
    
    // Calculate triplanar blend weights
    vec3 triblend = pow(abs(world_normal), vec3(4.0));
    triblend /= max(triblend.x + triblend.y + triblend.z, 0.0001);
    
    // Get world position for texture coordinates
    vec3 world_pos = (INV_VIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
    
    // Calculate triplanar UVs with scale and offset
    vec2 uvX = (world_pos.zy * texture_scale + texture_offset);
    vec2 uvY = (world_pos.xz * texture_scale + texture_offset);
    vec2 uvZ = (world_pos.xy * texture_scale + texture_offset);
    
    // Offset UVs to prevent obvious mirroring
    #ifdef TRIPLANAR_UV_OFFSET
        uvY += 0.33;
        uvZ += 0.67;
    #endif
    
    // Determine axis sign for UV correction
    vec3 axis_sign = sign(world_normal);
    
    // Flip UVs horizontally to correct for back side projection
    uvX.x *= axis_sign.x;
    uvY.x *= axis_sign.y;
    uvZ.x *= -axis_sign.z;
    
    // Sample textures for each projection direction
    vec4 colX = texture(albedo_texture, uvX);
    vec4 colY = texture(albedo_texture, uvY);
    vec4 colZ = texture(albedo_texture, uvZ);
    
    // Blend albedo
    vec4 col = colX * triblend.x + colY * triblend.y + colZ * triblend.z;
    
    // Sample and blend occlusion
    float occX = texture(occlusion_texture, uvX).g;
    float occY = texture(occlusion_texture, uvY).g;
    float occZ = texture(occlusion_texture, uvZ).g;
    float occ = mix(1.0, occX * triblend.x + occY * triblend.y + occZ * triblend.z, occlusion_strength);
    
    // Sample normal maps
    vec3 tnormalX = texture(normal_texture, uvX).rgb * 2.0 - 1.0;
    vec3 tnormalY = texture(normal_texture, uvY).rgb * 2.0 - 1.0;
    vec3 tnormalZ = texture(normal_texture, uvZ).rgb * 2.0 - 1.0;
    
    // Flip normal maps' x axis to account for flipped UVs
    tnormalX.x *= axis_sign.x;
    tnormalY.x *= axis_sign.y;
    tnormalZ.x *= -axis_sign.z;
    
    vec3 absVertNormal = abs(world_normal);
    
    // Apply reoriented normal mapping blend
    tnormalX = blend_rnm(vec3(world_normal.zy, absVertNormal.x), tnormalX);
    tnormalY = blend_rnm(vec3(world_normal.xz, absVertNormal.y), tnormalY);
    tnormalZ = blend_rnm(vec3(world_normal.xy, absVertNormal.z), tnormalZ);
    
    // Apply world space sign to tangent space Z
    tnormalX.z *= axis_sign.x;
    tnormalY.z *= axis_sign.y;
    tnormalZ.z *= axis_sign.z;
    
    // Sizzle tangent normals to match world normal and blend
    vec3 final_normal = normalize(
        tnormalX.zyx * triblend.x +
        tnormalY.xzy * triblend.y +
        tnormalZ.xyz * triblend.z
    );
    
    // Convert world normal to view space for Godot
    vec3 view_normal = normalize((VIEW_MATRIX * vec4(final_normal, 0.0)).xyz);
    
    // Set material properties
    ALBEDO = col.rgb;
    METALLIC = metallic;
    ROUGHNESS = 1.0 - smoothness; // Convert smoothness to roughness
    AO = occ;
    NORMAL = view_normal;
}