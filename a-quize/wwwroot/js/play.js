const ended = document.querySelector("[data-ended]")?.dataset.ended === "true";

if (!ended) {
    const questions = JSON.parse(document.querySelector("#questionsData").textContent);
    const endsAt = new Date(document.querySelector("#endsAtUtc").value);
    const questionTitle = document.querySelector("#questionTitle");
    const questionText = document.querySelector("#questionText");
    const options = document.querySelector("#options");
    const timer = document.querySelector("#timer");
    const progress = document.querySelector("#questionProgress");
    const answerStatus = document.querySelector("#answerStatus");
    const questionMap = document.querySelector("#questionMap");
    const answersJson = document.querySelector("#answersJson");
    const form = document.querySelector("#quizForm");
    const previous = document.querySelector("#previousQuestion");
    const next = document.querySelector("#nextQuestion");

    let currentIndex = 0;
    const answers = new Map();
    let hasSubmitted = false;

    previous.addEventListener("click", () => {
        if (currentIndex > 0) {
            currentIndex -= 1;
            renderQuestion();
        }
    });

    next.addEventListener("click", () => {
        if (currentIndex < questions.length - 1) {
            currentIndex += 1;
            renderQuestion();
        }
    });

    form.addEventListener("submit", () => {
        hasSubmitted = true;
        writeAnswers();
    });

    function renderQuestion() {
        const question = questions[currentIndex];
        questionTitle.textContent = `Question ${currentIndex + 1} of ${questions.length}`;
        questionText.textContent = question.Text;
        progress.textContent = `${currentIndex + 1} / ${questions.length}`;
        previous.disabled = currentIndex === 0;
        next.disabled = currentIndex === questions.length - 1;

        options.innerHTML = "";
        question.Options.forEach((option, index) => {
            const id = `option-${question.Id}-${index}`;
            const label = document.createElement("label");
            label.className = "choice";
            label.htmlFor = id;

            const radio = document.createElement("input");
            radio.type = "radio";
            radio.name = `question-${question.Id}`;
            radio.id = id;
            radio.value = index;
            radio.checked = answers.get(question.Id) === index;
            radio.addEventListener("change", () => {
                answers.set(question.Id, index);
                answerStatus.textContent = "Answer saved. You can still change it before submitting.";
                renderMap();
            });

            const span = document.createElement("span");
            span.textContent = option;

            label.appendChild(radio);
            label.appendChild(span);
            options.appendChild(label);
        });

        renderMap();
    }

    function renderMap() {
        questionMap.innerHTML = "";
        questions.forEach((question, index) => {
            const button = document.createElement("button");
            button.type = "button";
            button.className = "map-button";
            if (index === currentIndex) {
                button.classList.add("active");
            }
            if (answers.has(question.Id)) {
                button.classList.add("answered");
            }
            button.textContent = index + 1;
            button.addEventListener("click", () => {
                currentIndex = index;
                renderQuestion();
            });
            questionMap.appendChild(button);
        });
    }

    function writeAnswers() {
        const payload = Array.from(answers.entries()).map(([questionId, selectedOptionIndex]) => ({
            QuestionId: questionId,
            SelectedOptionIndex: selectedOptionIndex
        }));
        answersJson.value = JSON.stringify(payload);
    }

    function updateTimer() {
        if (Number.isNaN(endsAt.getTime())) {
            timer.textContent = "--:--";
            answerStatus.textContent = "Timer could not be loaded. Please reload the quiz.";
            return;
        }

        const secondsLeft = Math.max(0, Math.ceil((endsAt - new Date()) / 1000));
        const minutes = Math.floor(secondsLeft / 60).toString().padStart(2, "0");
        const seconds = (secondsLeft % 60).toString().padStart(2, "0");
        timer.textContent = `${minutes}:${seconds}`;

        if (secondsLeft === 0 && !hasSubmitted) {
            hasSubmitted = true;
            answerStatus.textContent = "Time is up. Submitting your quiz now.";
            writeAnswers();
            form.submit();
        }
    }

    renderQuestion();
    updateTimer();
    setInterval(updateTimer, 500);
}
