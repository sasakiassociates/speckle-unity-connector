using UnityEngine;
using UnityEngine.Rendering;

public class BufferRenderer : MonoBehaviour
{

	static Material lineMaterial;

	public int lineCount = 100;

	public float radius = 3.0f;

	// Will be called after all regular rendering is done
	public void OnRenderObject()
	{
		CreateLineMaterial();
		// Apply the line material
		lineMaterial.SetPass(0);

		GL.PushMatrix();
		// Set transformation matrix for drawing to
		// match our transform
		GL.MultMatrix(transform.localToWorldMatrix);

		// Draw lines
		GL.Begin(GL.LINES);
		for (var i = 0; i < lineCount; ++i)
		{
			var a = i / (float)lineCount;
			var angle = a * Mathf.PI * 2;
			// Vertex colors change from red to green
			GL.Color(new Color(a, 1 - a, 0, 0.8F));
			// One vertex at transform position
			GL.Vertex3(0, 0, 0);
			// Another vertex at edge of circle
			GL.Vertex3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
		}

		GL.End();
		GL.PopMatrix();
	}

	static void CreateLineMaterial()
	{
		if (!lineMaterial)
		{
			// Unity has a built-in shader that is useful for drawing
			// simple colored things.
			var shader = Shader.Find("Hidden/Internal-Colored");
			lineMaterial = new Material(shader);
			lineMaterial.hideFlags = HideFlags.HideAndDontSave;
			// Turn on alpha blending
			lineMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
			lineMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
			// Turn backface culling off
			lineMaterial.SetInt("_Cull", (int)CullMode.Off);
			// Turn off depth writes
			lineMaterial.SetInt("_ZWrite", 0);
		}
	}
}