namespace Treasara.Domain

/// <summary>
/// Module providing registry-based valuation capabilities for financial instruments.
/// </summary>
/// <remarks>
/// This module enables flexible valuation of different instrument types through a registry
/// pattern, where each instrument type can have its own projection strategy registered
/// dynamically. This approach supports extensibility and separation of concerns in the
/// valuation framework.
/// </remarks>
module RegistryValuation =

    /// <summary>
    /// Values a financial instrument by looking up its projector in the registry and applying valuation logic.
    /// </summary>
    /// <param name="registry">The projection registry containing registered projectors for various instrument types.</param>
    /// <param name="valuationDate">The date on which to perform the valuation.</param>
    /// <param name="dayCount">The day count convention to use for time calculations.</param>
    /// <param name="curve">The yield curve used for discounting future cash flows.</param>
    /// <param name="instrument">The instrument envelope containing the instrument to be valued.</param>
    /// <returns>
    /// A valuation result containing the present value, accrued interest, and projected cash flows
    /// for the specified instrument.
    /// </returns>
    /// <exception cref="DomainValidationException">
    /// Thrown when no projector has been registered for the instrument's type in the registry.
    /// This indicates that the valuation system does not yet support the given instrument type.
    /// </exception>
    /// <remarks>
    /// The valuation process follows these steps:
    /// <list type="number">
    /// <item><description>Attempts to retrieve a projector for the instrument type from the registry</description></item>
    /// <item><description>If found, generates a projection of the instrument's cash flows</description></item>
    /// <item><description>Applies the valuation logic using the provided date, day count, and yield curve</description></item>
    /// <item><description>Returns the computed valuation result</description></item>
    /// </list>
    /// This design allows for dynamic registration of new instrument types without modifying
    /// the core valuation logic, supporting the Open/Closed Principle.
    /// </remarks>
    let value
        (registry: ProjectionRegistry)
        (valuationDate: ValuationDate)
        (dayCount: DayCountConvention)
        (curve: YieldCurve)
        (instrument: InstrumentEnvelope)
        =

        match ProjectionRegistry.tryProject registry instrument with
        | Some projection ->
            Valuation.valueProjected valuationDate dayCount curve projection
        | None ->
            raise (
                DomainValidationException(
                    $"No projector registered for type {instrument.InstrumentType.Name}."
                )
            )