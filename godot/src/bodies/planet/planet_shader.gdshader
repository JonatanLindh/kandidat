shader_type spatial;

uniform float min_height;
uniform float max_height;
uniform sampler2D height_color;
uniform vec3 cliff_color = vec3(0.2);

varying float height;
varying vec3 dir_to_center;
varying vec3 normal;

void vertex() {
	height = length(VERTEX);
	dir_to_center = normalize(VERTEX - ((inverse(MODEL_MATRIX) * vec4(NODE_POSITION_WORLD, 1.0)).xyz));
	normal = NORMAL;
}

void fragment() {
	float t = (height / (max_height-min_height)) - (min_height / (max_height-min_height));
	t += 0.01;
	vec3 color = texture(height_color, vec2(t, 0.)).rgb;
	float slope = abs(dot(dir_to_center, normal));
	float cliff_amount = 1.0 - smoothstep(0.25, 0.45, slope);

	ALBEDO = mix(color, cliff_color, cliff_amount);
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
