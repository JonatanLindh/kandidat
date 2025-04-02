shader_type spatial;

render_mode cull_disabled, ensure_correct_normals;


group_uniforms GrassColor;
uniform vec3 base_color : source_color = vec3(0.05,0.2,0.01);
uniform vec3 tip_color : source_color = vec3(0.5,0.5,0.1);

// Source: https://www.shadertoy.com/view/Xt3cDn
float hash12(vec2 x) {
	uvec2 p = floatBitsToUint(x);
    p = 1103515245U * ((p >> 1U) ^ (p.yx));
    uint h32 = 1103515245U*((p.x) ^ (p.y >> 3U));
    uint n = h32 ^ (h32 >> 16U);
    return float(n) * (1.0 / float(0xffffffffU));
}

mat3 rotate_x(float angle) {
  float s = sin(angle), c = cos(angle);
  return mat3(vec3(1, 0, 0), vec3(0, c, s), vec3(0, -s, c));
}

mat3 rotate_y(float angle) {
  float s = sin(angle), c = cos(angle);
  return mat3(vec3(c, 0, -s), vec3(0, 1, 0), vec3(s, 0, c));
}


float ease_in_quartic(float x) {
	float a = x*x;
	return a*a;
}
float ease_out_quartic(float x) {
	float a = 1.0 - x;
	float b = a*a;
	return 1.0 - b*b;
}

void vertex() {
	// Called for every vertex the material is visible on.
	float height_percent = 1.0 - UV.y;
	float hash0 = hash12(NODE_POSITION_WORLD.xz);
	float hash1 = hash12(-NODE_POSITION_WORLD.zx);
	
	VERTEX.x *= mix(0.6, 1.0, hash1);
	
	// Rotate the grass blade
	float based_angle = 0.4;
	float turn_angle_base = mix(-based_angle, based_angle, hash0) * TAU;
	mat3 grass_rotate = rotate_y(turn_angle_base);

	// Bend the grass blade
	float curve_amount = hash0 * height_percent;
	mat3 grass_bend = rotate_x(curve_amount);
	
	mat3 grass_mat = grass_rotate * grass_bend;
	
	VERTEX = grass_mat * VERTEX;
	NORMAL = grass_mat * NORMAL;
	
}

void fragment() {
	// Called for every pixel the material is visible on.
	
	float height_factor = 1.0 - UV.y;
	ALBEDO = mix(base_color, tip_color, ease_in_quartic(height_factor));
	ALBEDO *= mix(0.1, 1.0, height_factor*height_factor);
	ROUGHNESS = mix(1.0, 0.4, ease_out_quartic(height_factor));
	ANISOTROPY = 0.85;
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
