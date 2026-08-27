using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

/// Ingheata atlasul fontului Caveat: il umple o data cu setul de caractere de
/// care avem nevoie, apoi il trece pe Static ca sa nu se mai schimbe singur.
///
/// De ce: fontul e pe atlas dinamic. Glifele lipsa se genereaza la rulare si
/// se scriu inapoi in asset, deci fisierul se modifica pe calculatorul
/// oricui a pornit jocul. A ajuns cu 30 de glife intr-un commit, 34 in
/// altul si 0 in al treilea, si se bate cu el insusi la fiecare push.
///
/// Dupa ce e Static, atlasul nu mai creste. Daca cineva scrie un caracter
/// care nu e in set, TMP nu-l deseneaza - de aceea unealta raporteaza clar
/// ce a intrat si ce a lipsit din fontul sursa.
///
/// Se poate rula din nou oricand se adauga caractere in set.
///
/// Meniu: LUCID / Font - ingheata atlasul Caveat
public static class InghetareFont
{
    const string Cale = "Assets/Caveat-VariableFont_wght SDF.asset";

    /// ASCII tiparibil, plus punctele de suspensie, plus diacriticele
    /// romanesti in ambele forme: cu virgula dedesubt (corect) si cu sedila
    /// (varianta veche, pe care o mai scot unele tastaturi).
    static string Setul()
    {
        var sb = new StringBuilder();

        for (char c = ' '; c <= '~'; c++) sb.Append(c);

        sb.Append('\u2026');                        // …
        sb.Append("\u0102\u0103\u00C2\u00E2\u00CE\u00EE");   // Ă ă Â â Î î
        sb.Append("\u0218\u0219\u021A\u021B");               // Ș ș Ț ț
        sb.Append("\u015E\u015F\u0162\u0163");               // Ş ş Ţ ţ

        return sb.ToString();
    }

    [MenuItem("LUCID/Font - ingheata atlasul Caveat")]
    public static void Ingheata()
    {
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Cale);

        if (font == null)
        {
            Debug.LogError($"Nu gasesc fontul la {Cale}.");
            return;
        }

        string set = Setul();

        // trebuie sa fie dinamic ca sa poata adauga glife din fontul sursa
        font.atlasPopulationMode = AtlasPopulationMode.Dynamic;
        font.ClearFontAssetData(false);

        bool tot = font.TryAddCharacters(set, out string lipsa);

        font.atlasPopulationMode = AtlasPopulationMode.Static;

        EditorUtility.SetDirty(font);
        AssetDatabase.SaveAssets();

        int puse = font.characterTable != null ? font.characterTable.Count : 0;

        Debug.Log($"Atlas inghetat: {puse} glife, {font.atlasWidth}x{font.atlasHeight}. " +
                  "Fontul e acum Static, nu se mai rescrie singur.", font);

        if (!tot && !string.IsNullOrEmpty(lipsa))
            Debug.LogWarning($"Fontul sursa nu are: '{lipsa}'. " +
                             "Caracterele astea NU se vor desena. Scrieti fara ele.", font);
        else
            Debug.Log("Tot setul a intrat, diacriticele romanesti incluse.", font);
    }
}
