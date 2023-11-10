namespace Gw2Manager.Models
{
    public class Command
    {
        public string Name { get; set; }
        public List<ConsoleKey> Keys { get; set; } = new();
        public int ListNumber { get; set; }
        public CommandType Type { get; set; }
        public CommandState State { get; set; }

        /// <summary>
        /// Pour une Commande Exec, renseigner le chemin de l'exe.
        /// Pour une Commande Github, renseigner ???.
        /// Pour une Commande Url, renseigner l'url de téléchargement.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Pour une Commande Exec, renseigner les arguments de lancement.
        /// Pour une Commande Github ou Url, renseigner le chemin complet de sortie.
        /// </summary>
        public string AdditionalData { get; set; }

        public bool IsLinkedToKey(ConsoleKey key)
        {
            return Keys.Contains(key);
        }
    }
}