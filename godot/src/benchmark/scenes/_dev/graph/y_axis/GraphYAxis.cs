using Godot;
using System;

public partial class GraphYAxis
{
	private int yAxisCount;
	private Label[] labels;
	private HSeparator[] separators;

	public GraphYAxis(VBoxContainer yAxisContainer, Panel panel, PackedScene yAxisLabel, PackedScene yAxisHSeparator, int yAxisCount)
	{
		this.yAxisCount = yAxisCount;
		labels = new Label[yAxisCount];
		separators = new HSeparator[yAxisCount];

		for (int i = 0; i < yAxisCount; i++)
		{
			Label label = yAxisLabel.Instantiate<Label>();
			yAxisContainer.AddChild(label);
			labels[i] = label;

			HSeparator separator = yAxisHSeparator.Instantiate<HSeparator>();
			panel.AddChild(separator);
			separators[i] = separator;
		}
	}

	/// <summary>
	/// Redraws the Y-axis labels and separators based on the maximum value.
	/// </summary>
	/// <param name="maxValue"></param>
	public void RedrawYAxis(float maxValue)
	{
		for (int i = 0; i < yAxisCount; i++)
		{
			Label cL = labels[i];
			HSeparator cS = separators[i];
			cL.Text = Math.Round(maxValue - (maxValue / yAxisCount * i), 0).ToString();

			float y = cL.Position.Y + cL.Size.Y / yAxisCount;
			cS.Position = new Vector2(0, y);
		}
	}
}
