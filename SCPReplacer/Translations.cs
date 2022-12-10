using Exiled.API.Interfaces;

namespace SCPReplacer
{
    public class Translations : ITranslation
    {
        public string WrongUsageMessage { get; set; } = "Usage: .volunteer <SCP number>. Example: .volunteer 079 or .v 079";
        public string TooLateMessage { get; set; } = "It is too late in the game to replace an SCP.";
        public string ChangedSuccessfullyMessage { get; set; } = "Changing your class to SCP-%NUMBER%.";
        public string ChangedSuccessfullySelfBroadcast { get; set; } = "You have replaced <color=red>SCP-%NUMBER%</color>";
        public string ChangedSuccessfullyEveryoneBroadcast { get; set; } = "<color=red>SCP-%NUMBER%</color> has been replaced";
        public string NoEligibleSCPsError { get; set; } = "No SCPs are currently eligible for replacement";
        public string InvalidSCPError { get; set; } = "The SCP number you entered is not availble. Currently available SCP numbers are: ";

        public string BroadcastHeader { get; set; } = "<color=yellow>[Chaos Theory SCP Replacer]</color>\n";

        public string ReplaceBroadcast { get; set; } = "Enter <color=green>.volunteer %NUMBER%</color> in the <color=orange>~</color> console to become <color=red>SCP-%NUMBER%</color>";
    }
}