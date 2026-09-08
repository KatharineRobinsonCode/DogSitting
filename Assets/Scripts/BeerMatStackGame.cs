// Left flat mat
_lastRoundSuccess = false;
yield return StartCoroutine(SlidingMatRound(flatMatWidth, flatMatHeight, flatMatY, leftTargetX, slideSpeed, "left"));
if (!_lastRoundSuccess)
{
    yield return StartCoroutine(FailSequence());
    yield break;
}
LastScore++;

// Right flat mat
_lastRoundSuccess = false;
yield return StartCoroutine(SlidingMatRound(flatMatWidth, flatMatHeight, flatMatY, rightTargetX, slideSpeed + speedIncrease, "right"));
if (!_lastRoundSuccess)
{
    yield return StartCoroutine(FailSequence());
    yield break;
}
LastScore++;

// Arch
_lastRoundSuccess = false;
yield return StartCoroutine(SlidingMatRound(pairGap + withinPairGap * 2f, flatMatHeight, archY, archTargetX, slideSpeed + speedIncrease * 2f, "arch"));
if (!_lastRoundSuccess)
{
    yield return StartCoroutine(FailSequence());
    yield break;
}
LastScore++;