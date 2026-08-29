using System;
using System.Collections.Generic;
using MoonscraperChartEditor.Song;

namespace YARG.Core.Chart
{
    internal partial class MoonSongLoader : ISongLoader
    {
        public InstrumentTrack<ProKeysNote> LoadProKeysTrack(Instrument instrument)
        {
            return LoadProKeysTrack(instrument, CreateProKeysNote);
        }

        private InstrumentTrack<ProKeysNote> LoadProKeysTrack(Instrument instrument, CreateNoteDelegate<ProKeysNote> createNote)
        {
            if (instrument.ToNativeGameMode() != GameMode.ProKeys)
                throw new ArgumentException($"Instrument {instrument} is not a pro-keys instrument!", nameof(instrument));

            var difficulties = new Dictionary<Difficulty, InstrumentDifficulty<ProKeysNote>>
            {
                { Difficulty.Easy,   LoadDifficulty(instrument, Difficulty.Easy, createNote, finalPassDelegate: ProKeysFinalPass) },
                { Difficulty.Medium, LoadDifficulty(instrument, Difficulty.Medium, createNote, finalPassDelegate: ProKeysFinalPass) },
                { Difficulty.Hard,   LoadDifficulty(instrument, Difficulty.Hard, createNote, finalPassDelegate: ProKeysFinalPass) },
                { Difficulty.Expert, LoadDifficulty(instrument, Difficulty.Expert, createNote, finalPassDelegate: ProKeysFinalPass) },
            };
            return new(instrument, difficulties);
        }

        private ProKeysNote CreateProKeysNote(MoonNote moonNote, Dictionary<MoonPhrase.Type, MoonPhrase> currentPhrases,
            List<ProKeysNote> notes)
        {
            var key = moonNote.proKeysKey;
            var generalFlags = GetGeneralFlags(moonNote, currentPhrases);
            var proKeysFlags = GetProKeysNoteFlags(moonNote, currentPhrases);

            double time = _moonSong.TickToTime(moonNote.tick);
            return new ProKeysNote(key, proKeysFlags, generalFlags, time, GetLengthInTime(moonNote),
                moonNote.tick, moonNote.length);
        }

        private ProKeysNoteFlags GetProKeysNoteFlags(MoonNote moonNote, Dictionary<MoonPhrase.Type, MoonPhrase> currentPhrases)
        {
            var flags = ProKeysNoteFlags.None;

            if (currentPhrases.TryGetValue(MoonPhrase.Type.ProKeys_Glissando, out var glissando) &&
                IsEventInPhrase(moonNote, glissando, inclusiveEnd: true))
            {
                // Sustains are not allowed in glissandos, so make sure the note has zero length
                moonNote.length = 0;

                flags |= ProKeysNoteFlags.Glissando;

                var previous = moonNote.PreviousSeperateMoonNote;
                var next = moonNote.NextSeperateMoonNote;

                if (previous is null || !IsEventInPhrase(previous, glissando, inclusiveEnd: true))
                {
                    flags |= ProKeysNoteFlags.GlissandoStart;
                }

                if (next is null || !IsEventInPhrase(next, glissando, inclusiveEnd: true))
                {
                    flags |= ProKeysNoteFlags.GlissandoEnd;
                }
            }

            return flags;
        }

        private static void ProKeysFinalPass(InstrumentDifficulty<ProKeysNote> chart)
        {
            var noteIndex = 0;

            // All we're here to do is assemble lane phrases, so if there aren't any notes or phrases, then we have nothing to do
            if (chart.Phrases.Count == 0 || chart.Notes.Count == 0)
            {
                return;
            }

            for (var phraseIndex = 0; phraseIndex < chart.Phrases.Count; phraseIndex++)
            {
                var phrase = chart.Phrases[phraseIndex];

                // Glissando phrases are handled earlier, since they don't require complex adjacent-note validation logic
                if (phrase.Type is not (PhraseType.TrillLane or PhraseType.TremoloLane))
                {
                    continue;
                }

                var notesInPhrase = GetNotesInLanePhrase(chart.Phrases, phraseIndex, chart.Notes, noteIndex, out noteIndex, false);

                List<ProKeysNote> laneNotes = phrase.Type switch
                {
                    PhraseType.TrillLane => GetProKeysTrillNotes(notesInPhrase),
                    PhraseType.TremoloLane => GetProKeysTremoloNotes(notesInPhrase),
                    _ => throw new ArgumentOutOfRangeException("Unreachable.")
                };

                if (laneNotes.Count > 0)
                {
                    // Apply lane start flag to entire starting note
                    foreach (var startChild in laneNotes[0].AllNotes)
                    {
                        startChild.ActivateFlag(NoteFlags.LaneStart);
                    }

                    // Cut sustains for all but the last note
                    foreach (var laneNote in laneNotes.GetRange(0, laneNotes.Count - 1))
                    {
                        foreach (var child in laneNote.AllNotes)
                        {
                            child.TickLength = 0;
                            child.TimeLength = 0;
                        }
                    }

                    // Apply lane end flag
                    foreach (var endChild in laneNotes[^1].AllNotes)
                    {
                        endChild.ActivateFlag(NoteFlags.LaneEnd);
                    }
                }

            }
        }

        // Takes all notes that are supposedly inside a PK tremolo phrase and validates them.
        // Activates the Tremolo flag for all notes in the phrase that constitute a valid tremolo
        //   -For a well-formed chart, this will be all of them
        //   -If the chart is malformed, the tremolo might terminate earlier than the supposed end of the phrase or be invalidated altogether
        // Returns the list of all marked notes. ProKeysFinalPass will assign the LaneStart and LaneEnd flags (shared behavior with trills)
        //
        // A PK tremolo must consist of exactly one notemask, repeated at least twice
        // If the tremolo's notemask changes on the second note, it's not a valid tremolo
        // If the notemask changes later in the tremolo, the tremolo is terminated there
        private static List<ProKeysNote> GetProKeysTremoloNotes(List<ProKeysNote> notesInPhrase)
        {
            static void AddToTremolo(ProKeysNote note, List<ProKeysNote> tremolo)
            {
                foreach (var child in note.AllNotes)
                {
                    child.ActivateFlag(NoteFlags.Tremolo);
                }

                tremolo.Add(note);
            }

            List<ProKeysNote> tremoloNotes = new();

            if (notesInPhrase.Count < 2 || notesInPhrase[0].NoteMask != notesInPhrase[1].NoteMask)
            {
                // This tremolo doesn't start with two matching masks, so it's invalid. Return an empty list (no tremolo)
                return tremoloNotes;
            }

            // The first two notes are now pre-cleared, so no need to check their masks again
            AddToTremolo(notesInPhrase[0], tremoloNotes);
            AddToTremolo(notesInPhrase[1], tremoloNotes);

            // Go through all further notes in the phrase and add them to the tremolo as long as the mask doesn't change
            for (var i = 2; i < notesInPhrase.Count; i++)
            {
                var note = notesInPhrase[i];

                if (notesInPhrase[i].NoteMask != notesInPhrase[0].NoteMask)
                {
                    // We found a mask that doesn't fit the tremolo; terminate early
                    break;
                }

                AddToTremolo(note, tremoloNotes);
            }

            return tremoloNotes;
        }


        // Takes all notes that are supposedly inside a Pro Keys trill phrase and validates them
        // Activates the Trill flag for all notes in the phrase that constitute a valid trill
        //   -For a well-formed chart, this will be all of them
        //   -If the chart is malformed, the trill might terminate earlier than the supposed end of the phrase or be invalidated altogether
        // Returns the list of all marked notes. ProKeysFinalPass will assign the LaneStart and LaneEnd flags (shared behavior with tremolos)
        //
        // A guitar trill must begin with two different single notes - if they match or either is a chord, the trill is invalid
        // The third note of the trill must match the first - if not, the trill is invalid
        // From there, the trill is valid as long as it continues alternating those two notes
        // If it repeats the same note twice, the trill terminates
        // If anything other than the two established notes occurs, the trill terminates
        private static List<ProKeysNote> GetProKeysTrillNotes(List<ProKeysNote> notesInPhrase)
        {
            List<ProKeysNote> trillNotes = new();

            if (notesInPhrase.Count < 3)
            {
                // Need at least three notes to constitute a trill; this is invalid. Return empty (no trill)
                return trillNotes;
            }

            if (notesInPhrase[0].IsChord || notesInPhrase[1].IsChord)
            {
                // Trills can't contain chords; this is invalid. Return empty (no trill)
                return trillNotes;
            }

            if (notesInPhrase[0].NoteMask == notesInPhrase[1].NoteMask)
            {
                // Trills must be two different notes; this is invalid. Return empty (no trill)
                return trillNotes;
            }

            if (notesInPhrase[0].NoteMask != notesInPhrase[2].NoteMask)
            {
                // The third note must match the first; this is invalid. Return empty (no trill)
                return trillNotes;
            }

            // We now know that we have a valid trill, and we've pre-approved the first three notes. Flag them and add them to the list
            notesInPhrase[0].ActivateFlag(NoteFlags.Trill);
            notesInPhrase[1].ActivateFlag(NoteFlags.Trill);
            notesInPhrase[2].ActivateFlag(NoteFlags.Trill);
            trillNotes.Add(notesInPhrase[0]);
            trillNotes.Add(notesInPhrase[1]);
            trillNotes.Add(notesInPhrase[2]);

            // The trill will continue for as long as it keeps alternating these two masks
            var mask = notesInPhrase[0].NoteMask;
            var otherMask = notesInPhrase[1].NoteMask;
            for (var i = 3; i < notesInPhrase.Count; i++)
            {
                var note = notesInPhrase[i];
                if (note.NoteMask == otherMask)
                {
                    note.ActivateFlag(NoteFlags.Trill);
                    trillNotes.Add(note);
                }
                else
                {
                    // We've stopped properly alternating; terminate the trill
                    break;
                }
                (mask, otherMask) = (otherMask, mask);
            }

            return trillNotes;
        }
    }
}