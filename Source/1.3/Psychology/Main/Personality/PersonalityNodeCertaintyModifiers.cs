using RimWorld;

namespace Psychology;

public class PersonalityNodeMemeCertaintyModifier
{
    // Meme for the node to interact with
    public MemeDef meme;
    // Given the meme, how the node rating affects certainty
    public float modifier;
}

public class PersonalityNodePreceptCertaintyModifier
{
    // Precept for the node to interact with
    public PreceptDef precept;
    // Given the precept, how the node rating affects certainty
    public float modifier;
}

