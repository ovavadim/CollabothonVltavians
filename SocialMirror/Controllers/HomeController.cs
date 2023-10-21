using System.Diagnostics;
using Firebase.Auth;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SocialMirror.Models;

namespace SocialMirror.Controllers
{
    public class HomeController : Controller
    {
        FirebaseAuthProvider auth;
        private readonly ILogger<HomeController> _logger;
        List<Question> Questions;
    
        string Username;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            InitQuestion();
            auth = new FirebaseAuthProvider(
                            new FirebaseConfig("AIzaSyBTfMkVACq1atf9TVLCkX3PerL9UuKKLts"));
        }


        void InitQuestion()
        {
            Questions = new List<Question>();
            //string[] lines = System.IO.File.ReadAllLines(@"..\SocialMirror\Resources\questions.txt");
            string[] lines = System.IO.File.ReadAllLines(@"Resources/questions.txt");

            foreach (string line in lines)
            {
                string[] quesAns = line.Split("|||");
                string[] quesParts = quesAns[0].Split("//");
                Question question = new Question()
                {
                    Id = Convert.ToInt32(quesParts[0]),
                    IsText = quesParts[1] == "textAns",
                    Text = quesParts[2],
                    ImagePath = quesParts[3]
                };

                string[] ansParts = quesAns[1].Split("///");

                foreach (string answer in ansParts)
                {
                    string[] textVal = answer.Split(";");
                    question.Answers.Add(new Answer
                    {
                        Text = textVal[0],
                        Value = textVal[1]
                    });
                }

                Questions.Add(question);

            }

        }
        public IActionResult Index()
        {

            return View();
        }

        public IActionResult Question()
        {

            Question ques = Questions.Where(p => p.Id == 1).ToList()[0];

            ViewData["newQues"] = ques;
            ViewData["buttonCaption"] = "Next Scenario";

            return View();
        }

        [HttpPost]
        public IActionResult Question(string quesId, string chosenAnswer)
        {
            int intId = Convert.ToInt32(quesId);


            //handle answer
            if (UserQuestionAnswer.OneUserAnswer == null)
                UserQuestionAnswer.OneUserAnswer = new List<UserOneAnswer>();


            UserQuestionAnswer.OneUserAnswer.Add(
                new UserOneAnswer
                {
                    User = UserInfo.userName,
                    Question = quesId,
                    Answer = chosenAnswer
                });
            

            


            //last page of the questionnary
            if (intId == Questions.Count - 1)
            {
                ViewData["buttonCaption"] = "Finish the questionnaire";

            }

            //user finished the questionnary
            if (intId == Questions.Count)
            {
                int points = 0;
                List<UserOneAnswer> currentUserAnswers = UserQuestionAnswer.OneUserAnswer.Where(p => p.User == UserInfo.userName).ToList();

                foreach(UserOneAnswer curAns in currentUserAnswers)
                {
                    points += Convert.ToInt32(curAns.Answer);
                }

                if(points > 0 && points <= 2)
                {
                    ViewData["Description"] = "Descr1";
                }

                if (points > 2 && points <= 4)
                {
                    ViewData["Description"] = "Descr2";
                }

                if (points > 4)
                {
                    ViewData["Description"] = "Descr3";
                }



                ViewData["Points"] = points;
                return View("Results");
            }

            ViewData["buttonCaption"] = "Next  Scenario";
            ++intId;

            ViewData["newQues"] = Questions.Where(p => p.Id == intId).ToList()[0];
            return View();
        }

        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registration(LoginModel loginModel)
        {
            try
            {
                Username = (loginModel.Email).Split('@')[0];
                ViewData["Username"] = Username;
                //create the user
                await auth.CreateUserWithEmailAndPasswordAsync(loginModel.Email, loginModel.Password);
                //log in the new user
                var fbAuthLink = await auth
                                .SignInWithEmailAndPasswordAsync(loginModel.Email, loginModel.Password);
                string token = fbAuthLink.FirebaseToken;
                //saving the token in a session variable
                if (token != null)
                {
                    HttpContext.Session.SetString("_UserToken", token);

                    return RedirectToAction("Privacy");
                }
            }
            catch (FirebaseAuthException ex)
            {
                var firebaseEx = JsonConvert.DeserializeObject<FirebaseError>(ex.ResponseData);
                ModelState.AddModelError(String.Empty, firebaseEx.error.message);
                return View(loginModel);
            }

            return View();

        }

        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(LoginModel loginModel)
        {
            try
            {
                
                //log in an existing user
                var fbAuthLink = await auth
                                .SignInWithEmailAndPasswordAsync(loginModel.Email, loginModel.Password);
                string token = fbAuthLink.FirebaseToken;
                //save the token to a session variable
                if (token != null)
                {
                    HttpContext.Session.SetString("_UserToken", token);
                    ViewData["Username"] = loginModel.Email;
                    UserInfo.userName = loginModel.Email;
                    return View("Privacy");
                }
                

            }
            catch (FirebaseAuthException ex)
            {
                var firebaseEx = JsonConvert.DeserializeObject<FirebaseError>(ex.ResponseData);
                ModelState.AddModelError(String.Empty, firebaseEx.error.message);
                return View(loginModel);
            }

            return View("Privacy");
        }

        public IActionResult LogOut()
        {
            HttpContext.Session.Remove("_UserToken");
            UserInfo.userName = null;
            return RedirectToAction("SignIn");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}