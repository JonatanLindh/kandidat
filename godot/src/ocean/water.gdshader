shader_type spatial;

uniform vec3 color : source_color;
uniform vec3 highlight_color : source_color;
uniform float metal : hint_range(0.0, 1.0, 0.01);
uniform float roughness : hint_range(0.0, 1.0, 0.01);
uniform float specular : hint_range(0.0, 1.0, 0.01);

uniform sampler2D bump_map1;
uniform sampler2D bump_map2;
uniform vec2 dir_bump_map1 = vec2(1,0);
uniform vec2 dir_bump_map2 = vec2(0,1);
uniform float speed_bump_map1 = 0.03;
uniform float speed_bump_map2 = 0.03;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	// Called for every pixel the material is visible on.

	vec3 bump1_color = texture(bump_map1, UV + (TIME * dir_bump_map1 * speed_bump_map1)).rgb;
	vec3 bump2_color = texture(bump_map2, UV + (TIME * dir_bump_map2 * speed_bump_map2)).rgb;
	vec3 interpolated_bump_color = mix(bump1_color, bump2_color, 0.5);
	NORMAL_MAP = interpolated_bump_color;
	
	ALBEDO = color;
	METALLIC = metal;
	ROUGHNESS = roughness;
	SPECULAR = specular;
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
