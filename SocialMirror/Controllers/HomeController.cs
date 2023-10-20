﻿using System.Diagnostics;
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
            string[] lines = System.IO.File.ReadAllLines(@"..\SocialMirror\Resources\questions.txt");
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

                string[] ansParts = quesAns[1].Split("//");

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
            //

            //Answer answer1 = new Answer()
            //{
            //    Text = "ans1",
            //    Value = "val1"
            //};

            //Answer answer2 = new Answer()
            //{
            //    Text = "ans2",
            //    Value = "val2"
            //};

            //Question question = new Question()
            //{
            //    Id = 1,
            //    Text = "ques1Text",
            //    Answers = new List<Answer>() { answer1, answer2 }
            //};

            ViewData["newQues"] = ques;

            return View();
        }

        [HttpPost]
        public IActionResult Question(string quesId, string chosenAnswer)
        {
            int intId = Convert.ToInt32(quesId);


            //handle answer



            if (intId == Questions.Count)
            {
                //last page

            }


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

                    return RedirectToAction("Index");
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