// res://tests/unit/HoldTimerTest.cs
// NOTA: Estos tests requieren gdUnit4 (ver GameManagerTest.cs). Se dejan comentados
// para permitir compilación total mientras gdUnit4 no esté instalado.

/*
using GdUnit4;
using static GdUnit4.Assertions;

namespace ElQuijote.Tests;

[TestSuite]
public class HoldTimerTest
{
    [TestCase]
    public void CompletesOnlyAfterHoldingFullDuration()
    {
        var timer = new HoldTimer(1.5f);
        AssertThat(timer.Update(true, 1.0f)).IsFalse();
        AssertThat(timer.Update(true, 0.6f)).IsTrue();
        AssertThat(timer.Progress).IsEqual(1f);
    }

    [TestCase]
    public void ReleasingDrainsProgress()
    {
        var timer = new HoldTimer(1.0f);
        timer.Update(true, 0.5f);
        timer.Update(false, 0.1f);
        AssertThat(timer.Progress).IsLess(0.5f);
    }

    [TestCase]
    public void ResetClearsProgress()
    {
        var timer = new HoldTimer(1.0f);
        timer.Update(true, 0.4f);
        timer.Reset();
        AssertThat(timer.Progress).IsEqual(0f);
    }
}
*/
