using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.Globalization;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.ApplicationModel;
using Windows.UI.Core;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SmartMirror.TestUCs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SpeechRecoginitionTest : Page
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

        //bool _recoEnabled = false;                                  // When this is true, we will continue to recognize  

        // Speech events may come in on a thread other than the UI thread, keep track of the UI thread's
        // dispatcher, so we can update the UI in a thread-safe manner.
        private CoreDispatcher dispatcher;


        public SpeechRecoginitionTest()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            populateLanguageDropdown();
            await initRecognizer((Language)((ComboBoxItem)cbLanguageSelection.SelectedItem).Tag);
        }

        private void populateLanguageDropdown()
        {
            cbLanguageSelection.SelectionChanged -= cbLanguageSelection_SelectionChanged;
            Language defaultLanguage = SpeechRecognizer.SystemSpeechLanguage;
            IEnumerable<Language> supportedLanguages = SpeechRecognizer.SupportedGrammarLanguages;
            foreach (Language lang in supportedLanguages)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Tag = lang;
                item.Content = lang.DisplayName;

                cbLanguageSelection.Items.Add(item);
                if (lang.LanguageTag == defaultLanguage.LanguageTag)
                {
                    item.IsSelected = true;
                    cbLanguageSelection.SelectedItem = item;
                }
            }
            cbLanguageSelection.SelectionChanged += cbLanguageSelection_SelectionChanged;
        }

        private async void cbLanguageSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await initRecognizer((Language)((ComboBoxItem)cbLanguageSelection.SelectedItem).Tag);
        }

        /// <summary>
        /// Initialize Speech Recognizer and compile constraints.
        /// </summary>
        /// <param name="recognizerLanguage">Language to use for the speech recognizer</param>
        /// <returns>Awaitable task.</returns>
        private async Task initRecognizer(Language recognizedLanguage)
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
                // Initialize the SRGS-compliant XML file.
                // For more information about grammars for Windows apps and how to
                // define and use SRGS-compliant grammars in your app, see
                // https://msdn.microsoft.com/en-us/library/dn596121.aspx

                // determine the language code being used.
                string languageTag = recognizedLanguage.LanguageTag;
                string fileName = String.Format("Assets\\Grammar\\{0}\\SampleGrammar.xml", languageTag);
                StorageFile grammarContentFile = await Package.Current.InstalledLocation.GetFileAsync(fileName);

                // Initialize the SpeechRecognizer and add the grammar.
                speechRecognizer = new SpeechRecognizer(recognizedLanguage);

                // Provide feedback to the user about the state of the recognizer. This can be used to provide
                // visual feedback to help the user understand whether they're being heard.
                speechRecognizer.StateChanged += SpeechRecognizer_StateChanged;

                SpeechRecognitionGrammarFileConstraint grammarConstraint = new SpeechRecognitionGrammarFileConstraint(grammarContentFile);
                SpeechRecognitionTopicConstraint tp = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, "command to execute");

                speechRecognizer.Constraints.Add(tp);
                SpeechRecognitionCompilationResult compilationResult = await speechRecognizer.CompileConstraintsAsync();

                // Check to make sure that the constraints were in a proper format and the recognizer was able to compile them.
                if (compilationResult.Status != SpeechRecognitionResultStatus.Success)
                {
                    //// Disable the recognition button.
                    //btnContinuousRecognize.IsEnabled = false;
                    //// Let the user know that the grammar didn't compile properly.
                    //resultTextBlock.Text = "Unable to compile grammar.";
                    System.Diagnostics.Debug.WriteLine("Unable to compile grammar, " + compilationResult.Status.ToString());
                }
                else
                {

                    // Set EndSilenceTimeout to give users more time to complete speaking a phrase.
                    speechRecognizer.Timeouts.EndSilenceTimeout = TimeSpan.FromHours(24);
                    speechRecognizer.Timeouts.InitialSilenceTimeout = TimeSpan.FromHours(24);
                    speechRecognizer.Timeouts.BabbleTimeout = TimeSpan.FromHours(24);
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
                if ((uint)ex.HResult == HResultRecognizerNotFound)
                {
                    //btnContinuousRecognize.IsEnabled = false;

                    resultTextBlock.Visibility = Visibility.Visible;
                    resultTextBlock.Text = "Speech Language pack for selected language not installed.";
                }
                else
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog(ex.Message, "Exception");
                    await messageDialog.ShowAsync();
                }
            }
        }



        //tbd
        private void SpeechRecognizer_StateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            //Nothing ATM
        }

        private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {

            if (args.Result.SemanticInterpretation != null)
            {
                if (args.Result.SemanticInterpretation.Properties.ContainsKey("FunctionToCall"))
                {
                    var str = args.Result.SemanticInterpretation.Properties["FunctionToCall"][0].ToString();
                    str = removetokens(str);
                    callTestfunction(str);
                }
            }
            else
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    resultTextBlock.Text = args.Result.Text;
                });



        }
        //endTBD
        private async void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                resultTextBlock.Text = "Recording finished, " + args.Status.ToString();
            });
           //await  speechRecognizer.RecognizeAsync();
        }


        /// <summary>
        /// normal not continius dictation scenario
        /// </summary>
        private async void initSpeechParts()
        {
            try
            {

                if (speechRecognizer == null)
                {


                    speechRecognizer = new SpeechRecognizer();
                    var lang = speechRecognizer.CurrentLanguage;
                    var functionGrammar = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, "Functioncall");
                    speechRecognizer.UIOptions.AudiblePrompt = "Give me the name of a Function";
                    speechRecognizer.UIOptions.ExampleText = "Function A";
                    speechRecognizer.Constraints.Add(functionGrammar);
                    await speechRecognizer.CompileConstraintsAsync();
                }

                try
                {
                    // Start recognition.

                    var messageDialog = new Windows.UI.Popups.MessageDialog("Give me a Function to call", "Command me");
                    var dialogAsync = messageDialog.ShowAsync();
                    SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeAsync();
                    dialogAsync.Cancel();
                    //seems a bit buggy, need to test a bit more
                    //SpeechRecognitionResult speechRecognitionResult = await _recognizer.RecognizeWithUIAsync();
                    var recognizedText = speechRecognitionResult.Text;
                    recognizedText = removetokens(recognizedText);
                    callTestfunction(recognizedText);
                }
                catch (Exception ex2)
                {
                    var ee = ex2.Message;
                    throw;
                }

            }
            catch
            (Exception ex)
            {
                //Check if user has approved the privacy policy
                const uint HresultPrivacyStatementDeclined = 0x80045509;
                if ((uint)ex.HResult == HresultPrivacyStatementDeclined)
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog(
                        "You must accept the speech privacy policy to continue", "Speech Exception");
                    messageDialog.ShowAsync().GetResults();
                }
            }
        }

        private string removetokens(string recognizedText)
        {
            return recognizedText.Replace("Function", "").Replace("Funktion", "").Replace(".", "").Trim();
        }


        #region "testFunctions"
        #region"Textual Output"
        private void callTestfunction(String letter)
        {
            var upper = letter.ToUpper();
            switch (upper)
            {
                case "A": testFunctionA(); break;
                case "B": testFunctionB(); break;
                case "C": testFunctionC(); break;
                case "1": testFunction1(); break;
                default: testFunctionNotAvailable(letter); break;
            }
        }


        private void testFunctionA()
        {
            showMessageBox("Function A was called", "Functioncall");
        }

        private void testFunctionB()
        {
            showMessageBox("Function B was called", "Functioncall");
        }

        private void testFunctionC()
        {
            showMessageBox("Function C was called", "Functioncall");
        }
        private void testFunction1()
        {
            showMessageBox("Function 1 was called", "Functioncall");
        }

        private void testFunctionNotAvailable(String letter)
        {
            showMessageBox("Named function, " + letter + " was not available", "Functioncall");
        }

        private async void showMessageBox(String message, string title)
        {
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
             {
                 var messageDialog = new Windows.UI.Popups.MessageDialog(
             message, "Functioncall");
                 await messageDialog.ShowAsync();
             });

        }

        #endregion

        #region "Vocal Output"

        #endregion

        #endregion

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            //initSpeechParts();

            button.IsEnabled = false;
            // The recognizer can only start listening in a continuous fashion if the recognizer is currently idle.
            // This prevents an exception from occurring.
            if (speechRecognizer.State == SpeechRecognizerState.Idle)
            {
                // Reset the text to prompt the user.
                try
                {
                    await speechRecognizer.ContinuousRecognitionSession.StartAsync();
                    ButtonText.Text = " Stop Continuous Recognition";
                    cbLanguageSelection.IsEnabled = false;
                }
                catch (Exception ex)
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog(ex.Message, "StartAsync Exception");
                    await messageDialog.ShowAsync();
                }
            }
            else
            {
                try
                {
                    // Reset the text to prompt the user.
                    ButtonText.Text = " Continuous Recognition";
                    cbLanguageSelection.IsEnabled = true;
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
            button.IsEnabled = true;
        }


    }
}
