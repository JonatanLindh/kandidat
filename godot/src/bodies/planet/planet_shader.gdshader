shader_type spatial;
render_mode cull_disabled;

uniform float min_height;
uniform float max_height;
uniform sampler2D height_color;
uniform vec3 cliff_color = vec3(0.2);
uniform vec3 chunk_center = vec3(0.0);  // Local center of this chunk relative to planet

varying float height;
varying vec3 dir_to_center;
varying vec3 normal;

void vertex() {
    // Calculate position relative to planet center (which is 0,0,0)
    vec3 relative_pos = VERTEX + chunk_center;
    
    // Calculate height as distance from planet center
    height = length(relative_pos);
    
    // Direction to center
    dir_to_center = normalize(-relative_pos);
    
    // Normal stays the same
    normal = NORMAL;
}

void fragment() {
	float t = (height / (max_height-min_height)) - (min_height / (max_height-min_height));
	t = clamp(t, 0.0, 1.0);
	vec3 color = texture(height_color, vec2(t, 0.)).rgb;
	float slope = abs(dot(dir_to_center, normal));
	float cliff_amount = 1.0 - smoothstep(0.25, 0.45, slope);

	ALBEDO = mix(color, cliff_color, cliff_amount);
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
