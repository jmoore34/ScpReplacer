using Exiled.API.Interfaces;

namespace SCPReplacer
{
    public class Translations : ITranslation
    {
        public string WrongUsageMessage { get; set; } = "Usage: .volunteer <SCP number>. Example: .volunteer 079 or .v 079";
        public string TooLateMessage { get; set; } = "It is too late in the game to replace an SCP.";
        public string ChangedSuccessfullyMessage { get; set; } = "Changing your class to SCP-%NUMBER%.";
        public string ChangedSuccessfullySelfBroadcast { get; set; } = "<color=yellow>[Chaos Theory SCP Replacer]</color>\nYou have replaced <color=red>SCP-%NUMBER%</color>";
        public string ChangedSuccessfullyEveryoneBroadcast { get; set; } = "<color=yellow>[Chaos Theory SCP Replacer]</color>\n<color=red>SCP-%NUMBER%</color> has been replaced";
        public string NotSuccessfully { get; set; } = "Unable to find a recently quit SCP with that SCP number";

        public string ReplaceBroadcast { get; set; } = "Enter <color=green>.volunteer %NUMBER%</color> in the <color=orange>~</color> console to become <color=red>SCP-%NUMBER%</color>";
    }
}