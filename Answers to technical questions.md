# Technical Assessment Feedback

## 1. How long did you spend on the coding assignment?
I was given one and a half days, of which I spent about 5 hours on the project. 

I wish I had more time to do way better, both in terms of clean code and architecture. I would also spend more time breaking down and cleaning bloated code in some parts of the project. I would use fixtures, replace the current implementation with Redis, and encrypt credentials to enhance security.

The reason I chose an in-memory database and this kind of cache is that it could be run with a single click with no configuration.

---

## 2. What would you add to your solution if you had more time? 
If you didn’t spend much time on the coding assignment, use this as an opportunity to explain what you would add.

I would add event sourcing and publish events to implement an event-driven architecture. I would create another project to act as a gateway.

---

## 3. How would you track down a performance issue in production? Have you ever had to do this?
I would identify which part of the system is causing the issue. I would check the resources for each service and locate bottlenecks. 

Steps I would follow:
- Analyze logs.
- Perform benchmarking.
- Conduct stress testing.

These steps would help identify the problematic areas.

---

## 4. What was the latest technical book you have read or tech conference you have been to?
I attended a DDD event last month hosted by Alibaba. 

I am also reading Vaughn Vernon's book on Domain-Driven Design and studying his ideas on event sourcing.

---

## 5. What do you think about this technical assessment?
It was a good assessment, but I wish I had more time.

---

## 6. Please, describe yourself using JSON.
```json
{
  "name": "Mohammad",
  "profession": "Senior Backend Developer",
  "experience": 6,
  "skills": ["Python", "C#", "DDD", "TDD", "Clean Architecture"],
  "projects": ["Sejam", "Contract Settlement"],
  "interests": ["AI", "philosophy"],
  "creative_goals": ["Comedy"],
  "tools": ["JetBrains Rider", "MassTransit", "Identity Server", "SQL Server"]
}
