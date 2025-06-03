document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("login-form");
    const messageP = document.getElementById("login-message");

    const buttonContainer = document.getElementById("post-login-buttons");
    const btnCreate = document.getElementById("go-create");
    const btnList = document.getElementById("go-list");

    form.addEventListener("submit", async (e) =>
    {
        e.preventDefault();

        const email = document.getElementById("email").value.trim();
        const password = document.getElementById("password").value;

        try
        {
            const res = await fetch("/auth/login",
            {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ email, password })
            });
            const data = await res.json();

            if (!res.ok)
            {
                const errorMsg = data.errors?.generalErrors?.[0] || data.message || "Login fehlgeschlagen";
                throw new Error(errorMsg || "Login fehlgeschlagen");
            }
            const { jwtToken, email: returnedEmail } = data

            // Save token to localStorage
            localStorage.setItem("jwt_token", jwtToken);

            messageP.textContent = "Login successful!";
            messageP.style.color = "green";

            // Show redirect buttons
            buttonContainer.style.display = "block";

        } catch (err)
        {
            console.error("Login error:", err);
            messageP.textContent = err.message;
            messageP.style.color = "red";
        }
    });

    // Redirect logic
    btnCreate.addEventListener("click", () =>
    {
        window.location.href = "create-session.html";
    });

    btnList.addEventListener("click", () =>
    {
        window.location.href = "trainingsessions.html";
    });
});