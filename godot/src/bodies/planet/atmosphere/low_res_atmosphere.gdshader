shader_type spatial;
render_mode unshaded, depth_test_disabled;

uniform vec3 atmosphere_color : source_color = vec3(0.5, 0.7, 1.0);
uniform float fresnel_power : hint_range(0.0, 10.0);

vec2 raySphereIntersect(vec3 ro, vec3 rd, vec3 so, float sr, float max_depth) {
	float t = dot(so-ro, rd);
	vec3 P = ro + rd * t;
	float y = length(so-P);
	
	if (y > sr){
		return vec2(-1.0);
	}
	
	float x = sqrt(sr*sr - y*y);
	float t1 = max(t-x, 0.0);
	float t2 = min(t+x,max_depth);
	
	return vec2(t1,t2);
}

void fragment() {
    float fresnel = pow(1.0 - dot(VIEW, NORMAL), fresnel_power);
    ALBEDO = atmosphere_color * fresnel;
    ALPHA = fresnel;
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
