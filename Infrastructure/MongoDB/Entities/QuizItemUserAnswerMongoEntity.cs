using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.MongoDB.Entities;

public class QuizItemUserAnswerMongoEntity : BaseMongoEntity
{
    [BsonElement( "userId" )]
    public int UserId { get; set; }

    [BsonElement( "quizItemId" )]
    public int QuizItemId { get; set; }

    [BsonElement( "quizItem" )]
    public QuizItemMongoEntity QuizItem { get; set; }

    [BsonElement( "quizId" )]
    public int QuizId { get; set; }

    [BsonElement( "userAnswer" )]
    public string UserAnswer { get; set; }
}