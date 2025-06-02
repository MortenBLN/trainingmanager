document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("login-form");
    const messageP = document.getElementById("login-message");

    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        const email = document.getElementById("email").value.trim();
        const password = document.getElementById("password").value;

        try {
            const res = await fetch("/auth/login", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ email, password })
            });

            if (!res.ok) {
                const error = await res.json();
                throw new Error(error.message || "Login failed");
            }
            const { jwtToken, email: returnedEmail } = await res.json();

            // Save token to localStorage
            localStorage.setItem("jwt_token", jwtToken);

            messageP.textContent = "Login successful!";
            messageP.style.color = "green";

            // Optional: redirect to create session page
            setTimeout(() => {
                window.location.href = "create-session.html";
            }, 1000);

        } catch (err) {
            console.error("Login error:", err);
            messageP.textContent = err.message;
            messageP.style.color = "red";
        }
    });
});