namespace FlightGame.Rendering.Core;

public class EffectSet
{
    private static readonly EffectSet _colored = new(
        "Colored",
        "ColoredInstanced",
        "ColoredInstancedDithered"
    );

    private static readonly EffectSet _waterTropical = new(
        "WaterTropical",
        null,
        null
    );

    private readonly string _standard;
    private readonly string? _instanced;
    private readonly string? _instancedDithered;

    private EffectSet(
        string standard,
        string? instanced,
        string? instancedDithered
    )
    {
        _standard = standard;
        _instanced = instanced;
        _instancedDithered = instancedDithered;
    }

    public static EffectSet Colored => _colored;

    public static EffectSet WaterTropical => _waterTropical;

    public void ApplyStandard(RenderContext renderContext, RenderParameters? parameters = null)
    {
        renderContext.Effect.CurrentTechnique = renderContext.Effect.Techniques[_standard];
        renderContext.GraphicsDeviceManager.PreferMultiSampling = true;

        ApplyRenderParameters(renderContext, parameters);
    }

    public void ApplyInstanced(RenderContext renderContext, RenderParameters? parameters = null)
    {
        if (_instanced == null)
        {
            throw new InvalidOperationException("This effect set does not support instanced rendering.");
        }

        renderContext.Effect.CurrentTechnique = renderContext.Effect.Techniques[_instanced];
        renderContext.GraphicsDeviceManager.PreferMultiSampling = true;

        ApplyRenderParameters(renderContext, parameters);
    }

    public void ApplyInstancedDithered(RenderContext renderContext, RenderParameters? parameters = null)
    {
        if (_instancedDithered == null)
        {
            throw new InvalidOperationException("This effect set does not support instanced dithered rendering.");
        }

        renderContext.Effect.CurrentTechnique = renderContext.Effect.Techniques[_instancedDithered];
        renderContext.GraphicsDeviceManager.PreferMultiSampling = false;

        ApplyRenderParameters(renderContext, parameters);
    }

    private static void ApplyRenderParameters(RenderContext renderContext, RenderParameters? parameters)
    {
        if (parameters != null)
        {
            renderContext.Effect.Parameters["xTime"]?.SetValue(parameters.GameTimeSeconds);
        }
    }
}
