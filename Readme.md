# Table of contents
* [StateMachine](#StateMachine)
* [StateGraphVisualizer](#StateGraphVisualizer)
* [ExampleUnityProject](#ExampleUnityProject)
<br><br>

<a name="StateMachine"></a>
# StateMachine
StateMachine folder - unity package with a finite state machine implementation.

State - describes the state of the module, what the module is doing at this moment, and what actions can be performed in this state.<br>
Transition - performs the logic of the transition between states.

```cs
class State : BaseState
{
    class Transition : BaseTransition
    {
        protected override ExecuteResult Execute()
        {
            // Transitions logic
            return new ExecuteResult { nextState = new State() }; // Determinate next state logic 
        }
    }

    protected override void OnEnter()
    {
        // Subscribtions
    }
    protected override void OnExit()
    {
        // Unsubscribtions
    }

    public void ExternalInteraction()
    {
        // Other logic
        
        Leave(new Transition());
    }
}
```

Get started with StateMachine:
```cs
    abstract class BaseState : Frameworks.StateMachine.BaseState<BaseState> { }
    
    abstract class BaseTransition<TContext> : Frameworks.StateMachine.BaseTransition<BaseState, TContext> { }
    abstract class BaseTransition : Frameworks.StateMachine.BaseTransition<BaseState> { }

    
    class StateMachine : Frameworks.StateMachine.BaseStateMachine<BaseState>
    {
        public StateMachine(BaseState initialState) : base(initialState) { }
    }
    
    public Component(/* dependencies */)
    {
        var statesContext = new Logic.StatesContext(/* dependencies */);
        var initialState = new Logic.ExampleState(statesContext);
        var stateMachine = new Logic.StateMachine(initialState);
        
        stateMachine.GetTypedState<Logic.ExampleState>().ExternalInteraction();
    }
```

For example see /ExampleUnityProject/Assets/Scripts/Match/.

<a name="StateGraphVisualizer"></a>
# StateGraphVisualizer 

StateGraphVisualizer folder - unity package with stateGraph detection logic.<br>
Microsoft.CodeAnalysis libraries (or Roslyn) uses for detection.

<div align="center">

![EditorWindow](https://habrastorage.org/r/w1560/webt/ce/k2/yr/cek2yr59xmwpguqifkcey165wdm.png)

</div>

Run on top panel Tools/StateMachine/Visualization.<br>
Select BaseState, BaseTransition and BaseTransition with context for determinate StateGraph.

The logic generates the `Graphviz` code for drawing diagrams.<br>
Use online editors for drawing `Graphviz` code. For examples:
- https://www.devtoolsdaily.com/graphviz
- https://graph.flyte.org

<div align="center">
    <img src="https://habrastorage.org/r/w780/webt/zf/0v/vr/zf0vvr2737crndo2w61umklhgoo.png" width="45%" />
    <img src="https://habrastorage.org/r/w780/webt/i1/cq/wz/i1cqwz4mmzhytfiucsznegaoeak.png" width="45%" />
</div>

<a name="ExampleUnityProject"></a>
# ExampleUnityProject
ExampleUnityProject folder - unity project with examples.
In the project:
- Connected OpenUPM witch Roslyn libraries (Microsoft.CodeAnalysis.CSharp library name in the Unity Package Manager).
- Connected the StateMachine
- Connected the StateGraphVisualizer
- `Match` feature with state machine сreated (see /ExampleUnityProject/Assets/Scripts/Match/)

Used Unity 6000.3.3f1 version.


