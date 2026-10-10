namespace BzCore.Routing;

/// <summary>
/// Cross-page transition played by <see cref="BzRouter"/>.
/// </summary>
/// <remarks>
/// <para><b>Why these are mostly opacity:</b> a CSS <c>transform</c> on an ancestor makes
/// that element the containing block for <c>position: fixed</c> descendants. Since the
/// route wrapper sits above every page, a transform here would pin dialogs, drawers and
/// lightboxes to the top of the document instead of the viewport.</para>
/// <para><see cref="FadeUp"/> and <see cref="Scale"/> do use a transform, so they finish
/// with <c>animation-fill-mode: none</c> and the wrapper is left untransformed. Prefer
/// <see cref="Fade"/> if a page hosts fixed-position UI.</para>
/// </remarks>
public enum BzRouteTransition
{
    /// <summary>No animation.</summary>
    None,

    /// <summary>Fade in. Safe with fixed-position content — the default.</summary>
    Fade,

    /// <summary>Fade while sliding up a few pixels.</summary>
    FadeUp,

    /// <summary>Fade while scaling up slightly.</summary>
    Scale,
}
