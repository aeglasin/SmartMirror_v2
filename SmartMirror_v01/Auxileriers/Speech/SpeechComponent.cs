using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using Windows.Storage;
using Windows.UI.Core;

namespace SmartMirror.Auxileriers.Speech
{
    class SpeechComponent
    {
        /// <summary>
        /// the HResult 0x8004503a typically represents the case where a recognizer for a particular language cannot
        /// be found. This may occur if the language is installed, but the speech pack for that language is not.
        /// See Settings -> Time & Language -> Region & Language -> *Language* -> Options -> Speech Language Options.
        /// </summary>
        private static uint HResultRecognizerNotFound = 0x8004503a;
        //SpeechSynthesizer _synthesizer;                             // The speech synthesizer (text-to-speech, TTS) object 
        SpeechRecognizer speechRecognizer;                               // The speech recognition object 
                                                                         //IAsyncOperation<SpeechRecognitionResult> _recoOperation;    // Used to canel the current asynchronous speech recognition operation 

        public delegate void CommandsGeneratedHandler(string command,string parameter);

        public delegate void SessionFinishedHandler();

        public event CommandsGeneratedHandler commandsGenerated;
        public event SessionFinishedHandler sessionsExpired;

        /// <summary>
        /// Initialize Speech Recognizer and compile constraints.
        /// </summary>
        /// <param name="recognizerLanguage">Language to use for the speech recognizer</param>
        /// <returns>Awaitable task.</returns>
        public async Task initRecognizer(Language recognizedLanguage)
        {


            if (speechRecognizer != null)
            {
                // cleanup prior to re-initializing this scenario.
                speechRecognizer.ContinuousRecognitionSession.Completed -= ContinuousRecognitionSession_Completed;
                speechRecognizer.ContinuousRecognitionSession.ResultGenerated -= ContinuousRecognitionSession_ResultGenerated;
                //speechRecognizer.ContinuousRecognitionSession.AutoStopSilenceTimeout = new TimeSpan(24, 0, 0);
                speechRecognizer.StateChanged -= SpeechRecognizer_StateChanged;
                this.speechRecognizer.Dispose();
                this.speechRecognizer = null;
            }

            try
            {
                // Initialize the SpeechRecognizer and add the grammar.
                speechRecognizer = new SpeechRecognizer(recognizedLanguage);

                // Provide feedback to the user about the state of the recognizer. This can be used to provide
                // visual feedback to help the user understand whether they're being heard.
                speechRecognizer.StateChanged += SpeechRecognizer_StateChanged;

               
                 string languageTag = recognizedLanguage.LanguageTag;
                languageTag = languageTag.Remove(languageTag.IndexOf("-"));
                string fileName = String.Format("Assets\\Grammar\\{0}\\Grammar.xml", languageTag);
                StorageFile grammarContentFile = await Package.Current.InstalledLocation.GetFileAsync(fileName);
                SpeechRecognitionGrammarFileConstraint grammarContstraint = new SpeechRecognitionGrammarFileConstraint(grammarContentFile);
                speechRecognizer.Constraints.Add(grammarContstraint);
                SpeechRecognitionCompilationResult compilationResult = await speechRecognizer.CompileConstraintsAsync();

                // Check to make sure that the constraints were in a proper format and the recognizer was able to compile them.
                if (compilationResult.Status != SpeechRecognitionResultStatus.Success)
                {
                    System.Diagnostics.Debug.WriteLine("Unable to compile grammar, " + compilationResult.Status.ToString());
                }
                else
                {

                    // Set EndSilenceTimeout to give users more time to complete speaking a phrase.
                    speechRecognizer.Timeouts.EndSilenceTimeout = TimeSpan.FromSeconds(1.2);
                    //speechRecognizer.Timeouts.InitialSilenceTimeout = TimeSpan.FromHours(1.2);
                    //speechRecognizer.Timeouts.BabbleTimeout = TimeSpan.FromHours(24);
                    // Handle continuous recognition events. Completed fires when various error states occur. ResultGenerated fires when
                    // some recognized phrases occur, or the garbage rule is hit.
                    speechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;
                    speechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;


                    //btnContinuousRecognize.IsEnabled = true;

                    //resultTextBlock.Text = speechResourceMap.GetValue("SRGSHelpText", speechContext).ValueAsString;
                    //resultTextBlock.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                if ((uint)ex.HResult == HResultRecognizerNotFound)
                {
                    //btnContinuousRecognize.IsEnabled = false;

                    message = "Speech Language pack for selected language not installed.";
                }
                var messageDialog = new Windows.UI.Popups.MessageDialog(message, "Exception");
                await messageDialog.ShowAsync();
            }
        }



        //tbd
        private void SpeechRecognizer_StateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            //Nothing ATM
        }

        private void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            if (args.Result.SemanticInterpretation != null)
            {
                string command="", param = "";
                if (args.Result.SemanticInterpretation.Properties.ContainsKey("command"))
                { command = args.Result.SemanticInterpretation.Properties["command"][0].ToString(); }
                if (args.Result.SemanticInterpretation.Properties.ContainsKey("param"))
                { param = args.Result.SemanticInterpretation.Properties["param"][0].ToString(); }

                commandsGenerated(command, param);
                System.Diagnostics.Debug.WriteLine("Something matched");
            }
            //commandsGenerated(extractCommands(args.Result.Text));
        }
        //endTBD
        private void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            sessionsExpired();
        }

        public async void startSession()
        {
            await initRecognizer(SpeechRecognizer.SystemSpeechLanguage);
            // The recognizer can only start listening in a continuous fashion if the recognizer is currently idle.
            // This prevents an exception from occurring.
            if (speechRecognizer.State == SpeechRecognizerState.Idle)
            {
                // Reset the text to prompt the user.
                try
                {
                    await speechRecognizer.ContinuousRecognitionSession.StartAsync();
                }
                catch (Exception ex)
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog(ex.Message, "StartAsync Exception");
                    await messageDialog.ShowAsync();
                }
            }
            else
            {
                throw new IllegaleStateException("not idle");
            }
        }

        public async void endSession()
        {
            try
            {
                // Cancelling recognition prevents any currently recognized speech from
                // generating a ResultGenerated event. StopAsync() will allow the final session to 
                // complete.
                await speechRecognizer.ContinuousRecognitionSession.CancelAsync();
            }
            catch (Exception ex)
            {
                var messageDialog = new Windows.UI.Popups.MessageDialog(ex.Message, "CancelAsync Exception");
                await messageDialog.ShowAsync();
            }
        }



        #region "FilterCommands"
        private Dictionary<SupportedCommands, List<string>> extractCommands(string input)
        {
            var tag = speechRecognizer.CurrentLanguage.LanguageTag;
            tag = tag.Remove(tag.IndexOf("-"));
            switch (tag.ToLower())
            {
                case "en": return extractCommandsEN(input);
                case "de": return extractCommandsDE(input);
                default: return null;
            }
        }
        #region "DE"
        private Dictionary<SupportedCommands, List<string>> extractCommandsDE(string input)
        {
            Dictionary<SupportedCommands, List<String>> retval = new Dictionary<SupportedCommands, List<String>>();
            extractMailCommandsDE(input, retval);
            extractCalenderCommandsDE(input, retval);
            return retval;
        }

        private void extractMailCommandsDE(string input, Dictionary<SupportedCommands, List<string>> retval)
        {

            //make it easier to filter
            input = input.ToLower();
            Regex rxShowMailList = new Regex("(zeig|gib (mir )?(alle )?(meine )?mail)");
            Regex rxParams;
            // rxParams= new Regex("(oder|und|von|an) ([A-Za-z0-9.@-]+ ?[A-Za-z0-9]*)");
            rxParams = new Regex("gib? mir? email|mail nummer? ([0-9]+)");
            if (rxShowMailList.IsMatch(input) & !rxParams.IsMatch(input))
            {

                retval.Add(SupportedCommands.showMailList, null);
            }
            else if (rxParams.IsMatch(input))
            {
                List<string> filters = new List<string>();
                foreach (Match item in rxParams.Matches(input))
                {
                    if (!(item.Groups.Count >= 2))
                    { continue; }

                    var filter = item.Groups[1].Value;
                    filters.Add(filter);
                }
                retval.Add(SupportedCommands.showMails, filters);
            }            
        }


        private void extractCalenderCommandsDE(string input, Dictionary<SupportedCommands, List<string>> retval)
        {
            //make it easier to filter
            input = input.ToLower();
            Regex rxOpenCalender = new Regex("zeig|zeige|gib|öffne mir? meinen? kalender");
            Regex rxCloseCalender = new Regex("(schließ|mach (meinen )?kalender( wieder)?( zu)?)|" +
                "kalender( wieder)? schließen");
            if (rxCloseCalender.IsMatch(input))
            { retval.Add(SupportedCommands.closeCalender, null); }
            else if (rxOpenCalender.IsMatch(input))
            { retval.Add(SupportedCommands.openCalender, null); }
        }
        #endregion

        #region "EN"

        private Dictionary<SupportedCommands, List<string>> extractCommandsEN(string input)
        {
            Dictionary<SupportedCommands, List<String>> retval = new Dictionary<SupportedCommands, List<String>>();
            extractMailCommandsEN(input, retval);
            extractCalenderCommandsEN(input, retval);
            return retval;
        }

        private void extractMailCommandsEN(string input, Dictionary<SupportedCommands, List<string>> retval)
        {

            //make it easier to filter
            input = input.ToLower();
            Regex rxShowMailList = new Regex("(show (me )?(all )?(my )?mail)");
            Regex rxParams;
            //rxParams= new Regex("(or|and|from|to) ([A-Za-z0-9.@-]+ ?[A-Za-z0-9]*)");
            rxParams = new Regex("give? me? mail number ([0-9]+)");
            if (rxShowMailList.IsMatch(input) & !rxParams.IsMatch(input))
            {
                retval.Add(SupportedCommands.showMailList, null);
            }
            else if (rxParams.IsMatch(input))
            {
                List<string> filters = new List<string>();
                foreach (Match item in rxParams.Matches(input))
                {
                    if (!(item.Groups.Count >= 2))
                    { continue; }

                    var filter = item.Groups[1].Value;
                    filters.Add(filter);
                }
                retval.Add(SupportedCommands.showMails, filters);
            }
        }
        private void extractCalenderCommandsEN(string input, Dictionary<SupportedCommands, List<string>> retval)
        {
            //make it easier to filter
            input = input.ToLower();
            Regex rxOpenCalender = new Regex("give|show|display (me )?(my )?calender)");
            Regex rxCloseCalender = new Regex("close (my )?calender( again)?");
            if (rxCloseCalender.IsMatch(input))
            { retval.Add(SupportedCommands.closeCalender, null); }
            else if (rxOpenCalender.IsMatch(input))
            { retval.Add(SupportedCommands.openCalender, null); }
        }
        private void extractNewsCommandsEN(string input, Dictionary<SupportedCommands, List<string>> retval)
        {
            input = input.ToLower();
            Regex rxOpenNewsComponent = new Regex("show? me? the? latest? news");
            Regex rxShowSpecificNews = new Regex("(give|show)? news number ([0-9]+)");
            Regex rxCloseAllNews = new Regex("close news");
            Regex rxcloseSpecificNews = new Regex("close news number ([0-9]+)");
            if (rxShowSpecificNews.IsMatch(input))
            {
                string[] listl = { rxShowSpecificNews.Match(input).Groups[2].Value };
                retval.Add(SupportedCommands.openSpecificNews, listl.ToList());
            }
            else if (rxOpenNewsComponent.IsMatch(input))
            {
                retval.Add(SupportedCommands.openNews, null);
            }
            else if (rxcloseSpecificNews.IsMatch(input))
            {
                string[] listl = { rxcloseSpecificNews.Match(input).Groups[1].Value };
                retval.Add(SupportedCommands.closeSpecificNews, listl.ToList());
            }
            else if (rxCloseAllNews.IsMatch(input))
            {
                retval.Add(SupportedCommands.closeNews, null);
            }

        }
        
        #endregion



        #endregion
    }
}
