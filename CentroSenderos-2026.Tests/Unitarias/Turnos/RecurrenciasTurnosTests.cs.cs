using System;
using CentroSenderos_2026_Shared.Enum;
using CentroSenderos_2026_Shared.Recurrencias;
using Xunit;

namespace CentroSenderos_2026.Tests.Unitarias.Turnos;

public class RecurrenciasTurnosTests
{
    #region Generación de fechas

    [Fact]
    public void SinRepeticion_DevuelveSoloLaFechaInicial()
    {
        var inicio = new DateTime(2026, 11, 2, 10, 30, 0);

        var fechas = CalculadorRecurrenciaTurno.CalcularFechas(
            inicio,
            EnumFrecuenciaRecurrenciaTurno.noRepite,
            null,
            1,
            null);

        Assert.Equal(new[] { inicio.Date }, fechas);
    }

    [Fact]
    public void Semanal_IncluyeLaFechaLimite()
    {
        var fechas = CalculadorRecurrenciaTurno.CalcularFechas(
            new DateTime(2026, 11, 2),
            EnumFrecuenciaRecurrenciaTurno.semanal,
            new DateTime(2026, 11, 16),
            1,
            null);

        Assert.Equal(new[]
        {
            new DateTime(2026, 11, 2),
            new DateTime(2026, 11, 9),
            new DateTime(2026, 11, 16)
        }, fechas);
    }

    [Fact]
    public void Semanal_NoGeneraFechasPosterioresAlLimite()
    {
        var fechas = CalculadorRecurrenciaTurno.CalcularFechas(
            new DateTime(2026, 11, 2),
            EnumFrecuenciaRecurrenciaTurno.semanal,
            new DateTime(2026, 11, 15),
            1,
            null);

        Assert.Equal(new[]
        {
            new DateTime(2026, 11, 2),
            new DateTime(2026, 11, 9)
        }, fechas);
    }

    [Fact]
    public void CadaQuinceDias_GeneraIntervalosDeQuinceDias()
    {
        var fechas = CalculadorRecurrenciaTurno.CalcularFechas(
            new DateTime(2026, 11, 2),
            EnumFrecuenciaRecurrenciaTurno.cadaQuinceDias,
            new DateTime(2026, 12, 2),
            1,
            null);

        Assert.Equal(new[]
        {
            new DateTime(2026, 11, 2),
            new DateTime(2026, 11, 17),
            new DateTime(2026, 12, 2)
        }, fechas);
    }

    [Fact]
    public void Mensual_FinDeMes_AjustaFebreroYConservaElDiaOriginalEnMarzo()
    {
        var fechas = CalculadorRecurrenciaTurno.CalcularFechas(
            new DateTime(2027, 1, 31),
            EnumFrecuenciaRecurrenciaTurno.mensual,
            new DateTime(2027, 3, 31),
            1,
            null);

        Assert.Equal(new[]
        {
            new DateTime(2027, 1, 31),
            new DateTime(2027, 2, 28),
            new DateTime(2027, 3, 31)
        }, fechas);
    }

    [Theory]
    [InlineData(EnumUnidadRecurrenciaTurno.dias, 3, 2026, 11, 5)]
    [InlineData(EnumUnidadRecurrenciaTurno.semanas, 2, 2026, 11, 16)]
    [InlineData(EnumUnidadRecurrenciaTurno.meses, 2, 2027, 1, 2)]
    public void Personalizada_RespetaIntervaloYUnidad(
        EnumUnidadRecurrenciaTurno unidad,
        int intervalo,
        int anio,
        int mes,
        int dia)
    {
        var inicio = new DateTime(2026, 11, 2);
        var siguiente = new DateTime(anio, mes, dia);

        var fechas = CalculadorRecurrenciaTurno.CalcularFechas(
            inicio,
            EnumFrecuenciaRecurrenciaTurno.personalizado,
            siguiente,
            intervalo,
            unidad);

        Assert.Equal(new[] { inicio, siguiente }, fechas);
    }

    #endregion

    #region Validaciones

    [Fact]
    public void Repeticion_SinFechaLimite_RechazaLaSolicitud()
    {
        Assert.Throws<ApplicationException>(() =>
            CalculadorRecurrenciaTurno.CalcularFechas(
                new DateTime(2026, 11, 2),
                EnumFrecuenciaRecurrenciaTurno.semanal,
                null,
                1,
                null));
    }

    [Fact]
    public void FechaLimiteAnteriorAlInicio_RechazaLaSolicitud()
    {
        Assert.Throws<ApplicationException>(() =>
            CalculadorRecurrenciaTurno.CalcularFechas(
                new DateTime(2026, 11, 2),
                EnumFrecuenciaRecurrenciaTurno.semanal,
                new DateTime(2026, 11, 1),
                1,
                null));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Personalizada_IntervaloNoPositivo_RechazaLaSolicitud(
        int intervalo)
    {
        Assert.Throws<ApplicationException>(() =>
            CalculadorRecurrenciaTurno.CalcularFechas(
                new DateTime(2026, 11, 2),
                EnumFrecuenciaRecurrenciaTurno.personalizado,
                new DateTime(2026, 11, 16),
                intervalo,
                EnumUnidadRecurrenciaTurno.dias));
    }

    [Fact]
    public void Personalizada_SinUnidad_RechazaLaSolicitud()
    {
        Assert.Throws<ApplicationException>(() =>
            CalculadorRecurrenciaTurno.CalcularFechas(
                new DateTime(2026, 11, 2),
                EnumFrecuenciaRecurrenciaTurno.personalizado,
                new DateTime(2026, 11, 16),
                1,
                null));
    }

    #endregion
}