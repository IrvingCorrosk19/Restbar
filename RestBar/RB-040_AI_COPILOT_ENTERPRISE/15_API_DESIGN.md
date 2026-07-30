# 15 — API DESIGN

MVC: GET /Copilot · POST /Copilot/Ask · GET /Copilot/History/{id}  
JSON Ask: { conversationId?, message } → { answerMarkdown, recommendations[], actions[], intent, durationMs }
