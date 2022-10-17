using FountainCSharp.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace FountainCSharp.Classes
{
    internal struct Token
    {
        public string Type { get; set; }
        public bool IsTitle { get; set; }
        public string Text { get; set; }
        public int? SceneNumber { get; set; }
        public string Dual { get; set; }
        public int Depth { get; set; }
    }

    public static class RegExes
    {

        public static Regex TitlePage = new Regex(@"^((?:title|credit|author[s]?|source|notes|draft date|date|contact|copyright))", RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline); //G

        public static Regex SceneHeading = new Regex(@"^((?:\*{0,3}_?)?(?:(?:int|ext|est|i\/e)[. ]).+)|^(?:\.(?!\.+))(.+)", RegexOptions.IgnoreCase);
        public static Regex SceneNumber = new Regex(@"( *#(.+)# *)/");

        public static Regex Transition = new Regex(@"^((?:FADE (?:TO BLACK|OUT)|CUT TO BLACK)\.|.+ TO\:)|^(?:> *)(.+)");

        public static Regex Dialogue = new Regex(@"^(?:([A-Z*_][0-9A-Z ._\-']*(?:\(.*\))?[ ]*)|\@([A-Za-z*_][0-9A-Za-z (._\-')]*))(\^?)?(?:\n(?!\n+))([\s\S]+)");
        public static Regex Parenthetical = new Regex(@"^(\(.+\))$");

        public static Regex Action = new Regex(@"^(.+)/g");
        public static Regex Centered = new Regex(@"^(?:> *)(.+)(?: *<)(\n.+)*");//G

        public static Regex Lyrics = new Regex(@"^~(?![ ]).+(?:\n.+)*");

        public static Regex Section = new Regex(@"^(#+)(?: *)(.*)");
        public static Regex Synopsis = new Regex(@"^(?:\=(?!\=+) *)(.*)");

        public static Regex Note = new Regex(@"^(?:\[{2}(?!\[+))(.+)(?:\]{2}(?!\[+))$");
        public static Regex NoteInline = new Regex(@"(?:\[{2}(?!\[+))([\s\S]+?)(?:\]{2}(?!\[+))");//G
        public static Regex Boneyard = new Regex(@"^(^\/\*|^\*\/)$");//G

        public static Regex PageBreak = new Regex(@"^\={3,}$");
        public static Regex LineBreak = new Regex(@"^ {2}$");

        public static Regex Emphasis = new Regex(@"(_|\*{1,3}|_\*{1,3}|\*{1,3}_)(.+)(_|\*{1,3}|_\*{1,3}|\*{1,3}_)");//G
        public static Regex BoldItalicUnderline = new Regex(@"(_{1}\*{3}(?=.+\*{3}_{1})|\*{3}_{1}(?=.+_{1}\*{3}))(.+?)(\*{3}_{1}|_{1}\*{3})");//G

        public static Regex BoldUnderline = new Regex(@"(_{1}\*{2}(?=.+\*{2}_{1})|\*{2}_{1}(?=.+_{1}\*{2}))(.+?)(\*{2}_{1}|_{1}\*{2})");//G
        public static Regex ItalicUnderline = new Regex(@"(_{1}\*{1}(?=.+\*{1}_{1})|\*{1}_{1}(?=.+_{1}\*{1}))(.+?)(\*{1}_{1}|_{1}\*{1})");//G
        public static Regex BoldItalic = new Regex(@"(\*{3}(?=.+\*{3}))(.+?)(\*{3})");//G
        public static Regex Bold = new Regex(@"(\*{2}(?=.+\*{2}))(.+?)(\*{2})");//G
        public static Regex Italic = new Regex(@"(\*{1}(?=.+\*{1}))(.+?)(\*{1})");//G
        public static Regex Underline = new Regex(@"(_{1}(?=.+_{1}))(.+?)(_{1})");//G

        public static Regex Splitter = new Regex(@"\n{2,}");//G
        public static Regex Cleaner = new Regex(@"^\n+|\n+$");
        public static Regex Standardizer = new Regex(@"\r\n|\r");//G
        public static Regex Whitespacer = new Regex(@"^\t+|^ {3,}", RegexOptions.Multiline);//G
    }

    public class Lexer
    {
        public static string Reconstrtuct(string script)
        {
            script = RegExes.Boneyard.Replace(script, "\n$1\n");
            script = RegExes.Standardizer.Replace(script, "\n");
            script = RegExes.Cleaner.Replace(script, "");
            script = RegExes.Whitespacer.Replace(script, "");
            return script;
        }
    }

    static class DefaultInlineLexer
    {
        internal static Dictionary<string, string> Properties = new Dictionary<string, string>
        {
            { "note", "<!-- $1 -->" },
            { "line_break", "<br />" },
            { "bold_italic_underline", @"<span class=""bold italic underline"">$2</span>"},
            { "bold_underline", @"<span class=""bold underline"">$2</span>"},
            { "italic_underline", @"<span class=""italic underline"">$2</span>"},
            { "bold_italic", @"<span class=""bold italic"">$2</span>"},
            { "bold", @"<span class=""bold"">$2</span>"},
            { "italic", @"<span class=""italic"">$2</span>"},
            { "underline", @"<span class=""underline"">$2</span>" },
        };
        public static string note => Items["note"];
        public static string line_break = Items["line_break"];
        public static string bold_italic_underline = Items["bold_italic_underline"];
        public static string bold_underline = Items["bold_underline"];
        public static string italic_underline = Items["italic_underline"];
        public static string bold_italic = Items["bold_italic"];
        public static string bold = Items["bold"];
        public static string italic = Items["italic"];
        public static string underline = Items["underline"];

    }

    public class InlineLexer : Lexer
    {
        /*
        public
        (
            string note,
            string line_break,
            string bold_italic_underline,
            string bold_underline,
            string italic_underline,
            string bold_italic,
            string bold,
            string italic,
            string underline
        ) Inline =
        (
            note: "<!-- $1 -->",
            line_break: "<br />",
            bold_italic_underline: @"<span class=""bold italic underline"">$2</span>",
            bold_underline: @"<span class=""bold underline"">$2</span>",
            italic_underline: @"<span class=""italic underline"">$2</span>",
            bold_italic: @"<span class=""bold italic"">$2</span>",
            bold: @"<span class=""bold"">$2</span>",
            italic: @"<span class=""italic"">$2</span>",
            underline: @"<span class=""underline"">$2</span>"
        );
        */
        public string Reconstruct(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return "";

            Regex match;
            var styles = new List<string> { "bold_italic_underline", "bold_underline", "italic_underline", "bold_italic", "bold", "italic", "underline" };

            line = RegExes.NoteInline.Replace(line, DefaultInlineLexer.note);
            line = new Regex(@"\\\*").Replace(line, "[star]");
            line = new Regex(@"\\_").Replace(line, "[underline]");
            line = new Regex(@"\n").Replace(line, DefaultInlineLexer.line_break);

            foreach (var style in styles)
            {
                match = new Regex(style);
                if (match.IsMatch(line))
                    line = match.Replace(line, DefaultInlineLexer.Properties[style]);
            }

            line = new Regex(@"\[star\]").Replace(line, "*");
            line = new Regex(@"\[underline\]").Replace(line, "_");

            return line;
        }


    }

    public class Scanner
    {
        private List<Token> Tokens = new List<Token>();

        public void Tokenize(string script)
        {
            var source = RegExes.Splitter.Split(Lexer.Reconstrtuct(script)).Reverse();
            string match;
            bool dual = false;

            foreach (var line in source)
            {

                //Title Page
                if (RegExes.TitlePage.IsMatch(line))
                {
                    match = RegExes.TitlePage.Replace(line, "\n$1");
                    var matches = RegExes.Splitter.Split(match).Reverse();
                    foreach (var item in matches)
                    {
                        var pair = new Regex(@"\:\n *").Split(RegExes.Cleaner.Replace(item, ""));

                        Tokens.Add(new Token
                        {
                            Type = pair[0].Trim().ToLower().Replace(' ', '_'),
                            IsTitle = true,
                            Text = pair[1].Trim()
                        });
                    }
                    continue;
                }

                //Scene Heading
                if (RegExes.SceneHeading.IsMatch(line))
                {
                    var matches = RegExes.SceneHeading.Matches(line);
                    var textes = !string.IsNullOrWhiteSpace(matches[0].Value) ? matches[0] : matches[1];
                    MatchCollection meta;
                    int? num = null;
                    string? text = null;

                    if (textes.Value.IndexOf(' ') != textes.Length - 2)
                    {
                        if (RegExes.SceneNumber.IsMatch(textes.Value))
                        {
                            meta = RegExes.SceneNumber.Matches(textes.Value);
                            if (int.TryParse(meta[0].Value, out int n))
                                num = n;
                            text = RegExes.SceneNumber.Replace(textes.Value, "");
                        }

                        Tokens.Add(new Token
                        {
                            Type = "scene_heading",
                            Text = text,
                            SceneNumber = num,
                        });

                    }
                }

                //Centered
                if(RegExes.Centered.IsMatch(line))
                {
                    var matches = RegExes.Centered.Match(line);
                    Tokens.Add(new Token
                    {
                        Type = "centered",
                        Text = new Regex(@">|<").Replace(matches.Value, ""),
                    });
                }

                // Transition
                if(RegExes.Transition.IsMatch(line))
                {
                    var matches = RegExes.Transition.Matches(line);
                    Tokens.Add(new Token
                    {
                        Type = "transition",
                        Text = !string.IsNullOrWhiteSpace(matches[0].Value) ? matches[0].Value : matches[1].Value,
                    });
                }




                // dialogue blocks - characters, parentheticals and dialogue
                if(RegExes.Dialogue.IsMatch(line))
                {
                    var matches = RegExes.Dialogue.Matches(line);
                    var nameMatch = !string.IsNullOrWhiteSpace(matches[0].Value) ? matches[0] : matches[1];
                    if(nameMatch.Value.IndexOf(' ') != nameMatch.Length)
                    {
                        if(!string.IsNullOrWhiteSpace(matches[3].Value))
                        {
                            Tokens.Add(new Token
                            {
                                Type = "dual_dialogue_end",
                            });
                        }
                        Tokens.Add(new Token
                        {
                            Type = "dialogue_end",
                        });

                        var parts = new Regex(@"").Split(matches[4].Value).Reverse();
                        foreach (var part in parts)
                        {
                            if(part.Length > 0)
                                Tokens.Add(new Token
                                {
                                    Type = RegExes.Parenthetical.IsMatch(part) ? "parenthetical" : "dialogue",
                                    Text = part,
                                });
                        }
                        Tokens.Add(new Token
                        {
                            Type = "character",
                            Text = nameMatch.Value.Trim(),

                        });
                        Tokens.Add(new Token
                        {
                            Type = "dialogue_begin",
                            Dual = !string.IsNullOrWhiteSpace(matches[3].Value) ? "right" : dual ? "left" : null,

                        });
                        if (dual)

                        Tokens.Add(new Token
                        {
                            Type = "dual_dialogue_begin",
                        });

                        dual = !string.IsNullOrWhiteSpace(matches[3].Value);
                        continue;
                    }
                }




                /** section */
                if (match = line.match(regex.section))
                {
                    this.tokens.push({ type: 'section', text: match[2], depth: match[1].length });
                    continue;
                }

                /** synopsis */
                if (match = line.match(regex.synopsis))
                {
                    this.tokens.push({ type: 'synopsis', text: match[1] });
                    continue;
                }
            }
    }
}

export class Scanner
{
    private tokens: Token[] = [];

    tokenize(script: string) : Token[] {
        // reverse the array so that dual dialog can be constructed bottom up
        const source: string[] = new Lexer().reconstruct(script).split(regex.splitter).reverse();

    let line: string;
        let match: string[];
        let dual: boolean;

        for (line of source) {




/** notes */
if (match = line.match(regex.note))
{
    this.tokens.push({ type: 'note', text: match[1] });
    continue;
}

/** boneyard */
if (match = line.match(regex.boneyard))
{
    this.tokens.push({ type: match[0][0] === '/' ? 'boneyard_begin' : 'boneyard_end' });
    continue;
}

/** lyrics */
if (match = line.match(regex.lyrics))
{
    this.tokens.push({ type: 'lyrics', text: match[0].replace(/ ^~(? ![ ]) / gm, '') });
    continue;
}

/** page breaks */
if (regex.page_break.test(line))
{
    this.tokens.push({ type: 'page_break' });
    continue;
}

/** line breaks */
if (regex.line_break.test(line))
{
    this.tokens.push({ type: 'line_break' });
    continue;
}

// everything else is action -- remove `!` for forced action
this.tokens.push({ type: 'action', text: line.replace(/ ^!(? ![ ]) / gm, '') });
        }

        return this.tokens.reverse();
    }
}
