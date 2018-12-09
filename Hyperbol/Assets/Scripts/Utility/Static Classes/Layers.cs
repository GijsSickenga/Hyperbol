/// <summary>
/// Used to convert from named collision layers to layer integer id's.
/// </summary>
public static class Layers
{
	public enum Names
    {
        // Unity's built-in layers.
        Default = 0,
        TransparentFX = 1,
        IgnoreRaycast = 2,
        Water = 4,
        UI = 5,

        // Custom layers.
        PostProcessing = 8,
        Ships = 9,
        Hyperbol = 10
    }
}
