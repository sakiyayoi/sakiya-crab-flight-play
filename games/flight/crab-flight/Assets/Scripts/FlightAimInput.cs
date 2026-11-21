using System;
[Serializable] public sealed class FlightAimInput { public float angle=40,power=70,spinRps=0,spinPhase=0;public int direction=1;public bool eligible=true; }
public static class SwipeFlightRules {public const string RuleVersion="crab-r22-contact-cause1";public const float MaxSpinRps=12;}
