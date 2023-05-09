using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Infrastructure.MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.MongoDB.Entities;

public class QuizUserServiceMongoDB : IQuizUserService
{
    private readonly IMongoCollection<QuizMongoEntity> _quizzes;
    private readonly IMongoCollection<QuizItemUserAnswerMongoEntity> _answers;
    private readonly MongoClient _client;

    public QuizUserServiceMongoDB(IOptions<MongoDBSettings> settings)
    {
        _client = new MongoClient( settings.Value.ConnectionUri );
        IMongoDatabase database = _client.GetDatabase( settings.Value.DatabaseName );
        _quizzes = database.GetCollection<QuizMongoEntity>( settings.Value.QuizCollection );
        _answers = database.GetCollection<QuizItemUserAnswerMongoEntity>( settings.Value.AnswersCollection );
    }

    public Quiz? FindQuizById(int id)
    {
        return _quizzes
            .Find( Builders<QuizMongoEntity>.Filter.Eq( q => q.QuizId, id ) )
            .Project( q =>
                new Quiz(
                    q.QuizId,
                    q.Items.Select( i => new QuizItem(
                            i.ItemId,
                            i.Question,
                            i.IncorrectAnswers,
                            i.CorrectAnswer
                        )
                    ).ToList(),
                    q.Title
                )
            ).FirstOrDefault();
    }

    public List<QuizItemUserAnswer> GetUserAnswersForQuiz(int quizId, int userId)
    {
        var userFilter = Builders<QuizItemUserAnswerMongoEntity>.Filter.Eq( q => q.UserId, userId );
        var quizFilter = Builders<QuizItemUserAnswerMongoEntity>.Filter.Eq( q => q.QuizId, quizId );

        var userQuizFilter = Builders<QuizItemUserAnswerMongoEntity>.Filter.And( userFilter, quizFilter );

        var quizItemTest = new QuizItem( id: 1, question: "1+4", incorrectAnswers: new List<string>() { "2", "1", "4" }
        , correctAnswer: "5" );

        var answers = _answers.Find( userQuizFilter )
            .Project( q => new QuizItemUserAnswer(
                quizItemTest,
                 q.UserId,
                q.QuizId,
                 q.UserAnswer
                ) ).ToList();

        return answers;
    }

    public void SaveUserAnswerForQuiz(int quizId, int userId, int quizItemId, string answer)
    {
        QuizItemUserAnswerMongoEntity newAnswer = new QuizItemUserAnswerMongoEntity
        {
            UserAnswer = answer,
            QuizItemId = quizItemId,
            QuizId = quizId,
            UserId = userId
        };

        _answers.InsertOne( newAnswer );
    }

    public IEnumerable<Quiz> FindAllQuizzess()
    {
        var quizMongoEntities = _quizzes.Find( Builders<QuizMongoEntity>.Filter.Empty ).ToList();
        return _quizzes
            .Find( Builders<QuizMongoEntity>.Filter.Empty )
            .Project(
                q =>
                    new Quiz(
                        q.QuizId,
                        q.Items.Select( i => new QuizItem(
                                i.ItemId,
                                i.Question,
                                i.IncorrectAnswers,
                                i.CorrectAnswer
                            )
                        ).ToList(),
                        q.Title
                    )
            ).ToEnumerable();
    }

    public Quiz CreateAndGetQuizRandom(int count)
    {
        throw new NotImplementedException();
    }
}