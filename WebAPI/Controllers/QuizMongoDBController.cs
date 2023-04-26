using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Infrastructure.MongoDB.Entities;
using Infrastructure.MongoDB.Service;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;
[ApiController, Route( "/api/v2/mongodb" )]
public class QuizMongoDBController : ControllerBase
{
    private readonly QuizUserServiceMongoDB _service;


    public QuizMongoDBController(QuizUserServiceMongoDB service)
    {
        _service = service;
    }

    [HttpGet("/quidId")]
    public ActionResult<Quiz> GetQuiz(int quizId)
    {
        var quiz = _service.FindQuizById(quizId);
        if(quiz == null)
        {
            return NotFound();
        }
        return quiz;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Quiz>> GetAllQuiz()
    {
        return Ok(_service.FindAllQuizzess());
    }
}
