using Xunit.Sdk;
using Xunit.v3;

// Las clases de integración y UI comparten una BD y un servidor local.
// Esta suite funcional se ejecuta en serie; no certifica seguridad bajo concurrencia.
// Mantener aparte el hallazgo de colisión de afiliados (MAX + 1).
[assembly: Parallelization(Mode = ParallelMode.None)]
