using System;

interface IGameplayProgression
{
    event Action onPointsAdded;
    int points { get; }
}