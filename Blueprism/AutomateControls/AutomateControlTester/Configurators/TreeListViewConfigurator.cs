using System;
using AutomateControls.TreeList;

namespace AutomateControlTester.Configurators
{
    public partial class TreeListViewConfigurator : BaseConfigurator
    {
        public TreeListViewConfigurator()
        {
            InitializeComponent();
        }

        private void AddFilm(TreeListViewItemCollection items,
            string name, string description, int score)
        {
            var item = items.Add(name, -1);
            item.SubItems.Add(description);
            item.SubItems.Add(score + "%");
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            var root = tvFilms.Items.Add("Films", -1);
            AddFilm(root.Items, "Citizen Kane",
                "Orson Welles's epic tale of a publishing tycoon's rise and fall " +
                "is entertaining, poignant, and inventive in its storytelling, " +
                "earning its reputation as a landmark achievement in film.", 100);

            var scifi = root.Items.Add("Sci Fi", -1);
            AddFilm(scifi.Items, "Blade Runner",
                "Ridley Scott's masterpiece; A scifi noir set in a " +
                "near future Los Angeles", 90);

            AddFilm(scifi.Items, "Gattaca",
                "A young man's lifetime quest to become an astronaut in a " +
                "futuristic setting that disallows progression of inadequate " +
                "humans to higher societal ranks.", 82);

            AddFilm(scifi.Items, "Alien",
                "A modern classic, Alien blends science fiction, horror and " +
                "bleak poetry into a seamless whole.", 97);

            var comedy = root.Items.Add("Comedy", -1);
            AddFilm(comedy.Items, "Ghostbusters",
                "An infectiously fun blend of special effects and comedy, with "+
                "Bill Murray's hilarious deadpan performance leading a cast of "+
                "great comic turns.", 97);

            AddFilm(comedy.Items, "Peter's Friends",
                "Set in a beautiful English country manor, this provocative "+
                "ensemble comedy drama chronicles the New Year's Eve 10-year "+
                "reunion of a group of former members of the Oxford theatre "+
                "department.", 71);

            AddFilm(comedy.Items, "In The Bleak Midwinter",
                "When his professional career hits a lull, an actor attempts "+
                "to revitalize his career by staging a production of Hamlet, "+
                "directed by and starring himself", 81);

        }

        public override string ConfigName
        {
            get { return "Tree List View"; }
        }

    }
}
