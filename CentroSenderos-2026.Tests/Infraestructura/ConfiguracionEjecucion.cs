using Xunit;

// Las clases de integración y UI comparten una BD y un servidor local.
// Esta suite funcional se ejecuta en serie; no certifica seguridad bajo concurrencia.
// Mantener aparte el hallazgo de colisión de afiliados (MAX + 1).
[assembly: CollectionBehavior(DisableTestParallelization = true)]
