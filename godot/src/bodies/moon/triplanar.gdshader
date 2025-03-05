shader_type spatial;

uniform sampler2D albedo : source_color;
uniform sampler2D normal: hint_normal;

uniform float blend_sharpness = 8.0;
uniform float mapping_scale = 1.0;

void fragment() {
    vec3 world_pos = (INV_VIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
    vec3 world_normal = normalize((INV_VIEW_MATRIX * vec4(NORMAL, 0.0)).xyz);

    vec3 blend_weights = pow(abs(world_normal), vec3(blend_sharpness));

    blend_weights = blend_weights / (blend_weights.x + blend_weights.y + blend_weights.z);

    vec3 scaled_pos = world_pos * mapping_scale;

    vec3 col_x = texture(albedo, scaled_pos.yz).rgb;
    vec3 col_y = texture(albedo, scaled_pos.xz).rgb;
    vec3 col_z = texture(albedo, scaled_pos.xy).rgb;


    vec3 tnormal_x = texture(normal, scaled_pos.yz).rgb * 2.0 - 1.0;
    vec3 tnormal_y = texture(normal, scaled_pos.xz).rgb * 2.0 - 1.0;
    vec3 tnormal_z = texture(normal, scaled_pos.xy).rgb * 2.0 - 1.0;

    vec3 world_normal_x = vec3(world_normal.x >= 0.0 ? tnormal_x.z : -tnormal_x.z,
                               tnormal_x.y,
                               world_normal.x >= 0.0 ? tnormal_x.x : -tnormal_x.x);

    vec3 world_normal_y = vec3(tnormal_y.x,
                               world_normal.y >= 0.0 ? tnormal_y.z : -tnormal_y.z,
                               world_normal.y >= 0.0 ? tnormal_y.y : -tnormal_y.y);

    vec3 world_normal_z = vec3(tnormal_z.x,
                               tnormal_z.y,
                               world_normal.z >= 0.0 ? tnormal_z.z : -tnormal_z.z);

    vec3 tangent_x = normalize(vec3(0.0, -world_normal_x.z, world_normal_x.y));
    vec3 bitangent_x = normalize(cross(world_normal_x, tangent_x));
    mat3 tbn_x = mat3(tangent_x, bitangent_x, normalize(world_normal_x));

    vec3 tangent_y = normalize(vec3(-world_normal_y.z, 0.0, world_normal_y.x));
    vec3 bitangent_y = normalize(cross(world_normal_y, tangent_y));
    mat3 tbn_y = mat3(tangent_y, bitangent_y, normalize(world_normal_y));

    vec3 tangent_z = normalize(vec3(-world_normal_z.y, world_normal_z.x, 0.0));
    vec3 bitangent_z = normalize(cross(world_normal_z, tangent_z));
    mat3 tbn_z = mat3(tangent_z, bitangent_z, normalize(world_normal_z));

    vec3 normal_x_world = normalize(tbn_x * tnormal_x);
    vec3 normal_y_world = normalize(tbn_y * tnormal_y);
    vec3 normal_z_world = normalize(tbn_z * tnormal_z);

    vec3 final_normal = normalize(
        normal_x_world * blend_weights.x +
        normal_y_world * blend_weights.y +
        normal_z_world * blend_weights.z
    );

    vec3 final_color = col_x * blend_weights.x + col_y * blend_weights.y + col_z * blend_weights.z;

    ALBEDO = final_color;
    NORMAL = final_normal;
    ROUGHNESS = 0.7;
}