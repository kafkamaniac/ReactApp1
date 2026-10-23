import { useState } from "react";
import "./App.css";

function App() {
  const [isRegister, setIsRegister] = useState(false);
  const [login, setLogin] = useState("");
  const [password, setPassword] = useState("");
  const [repeatPassword, setRepeatPassword] = useState("");
  const [message, setMessage] = useState("");
  const [captchaRequired, setCaptchaRequired] = useState(false);
  const [captchaNumber, setCaptchaNumber] = useState(0);
  const [captchaInput, setCaptchaInput] = useState("");
  const [captchaMessage, setCaptchaMessage] = useState("");

  //#region  РЕГИСТРАЦИЯ
  async function register() {
    setMessage("");

    if (password !== repeatPassword) {
      setMessage("Пароли не совпадают.");
      return;
    }

    try {
      const response = await fetch("/api/auth/register", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          login: login,
          password: password,
        }),
      });

      const data = await response.json();

      if (!response.ok) {
        setMessage(data.message || "Ошибка регистрации.");
        return;
      }

      setMessage(data.message);
      setPassword("");
      setRepeatPassword("");
    } catch {
      setMessage("Не удалось подключиться к серверу.");
    }
  }
  //#endregion

  //#region  ВХОД
  async function loginUser() {
    setMessage("");
    setCaptchaNumber(Math.floor(1000 + Math.random() * 9000));
    setCaptchaInput("");
    setCaptchaMessage("");
    setCaptchaRequired(true);

    try {
      const response = await fetch("/api/auth/login", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          login: login,
          password: password,
        }),
      });

      const data = await response.json();

      if (!response.ok) {
        setMessage(data.message || "Ошибка входа.");
        return;
      }
    } catch {
      setMessage("Не удалось подключиться к серверу.");
    }
  }

  function checkCaptcha() {
    if (captchaInput === String(captchaNumber)) {
      setCaptchaMessage("Проверка пройдена!");
      setCaptchaRequired(false);
      setMessage("Вход выполнен успешно.");
    } else {
      setCaptchaMessage("Неверное число. Попробуйте ещё раз.");
      setCaptchaNumber(Math.floor(1000 + Math.random() * 9000));
      setCaptchaInput("");
    }
  }

  //#endregion
  //#region интерфейс

  return (
    <div className="container">
      {isRegister ? (
        <>
          <h1>Регистрация</h1>

          <input
            type="text"
            placeholder="Придумайте логин"
            value={login}
            onChange={(e) => setLogin(e.target.value)}
          />

          <input
            type="password"
            placeholder="Придумайте пароль"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />

          <input
            type="password"
            placeholder="Повторите пароль"
            value={repeatPassword}
            onChange={(e) => setRepeatPassword(e.target.value)}
          />

          <button onClick={register}>Зарегистрироваться</button>

          <button
            onClick={() => {
              setIsRegister(false);
              setMessage("");
            }}
          >
            Назад ко входу
          </button>
        </>
      ) : (
        <>
          <h1>Вход</h1>

          <input
            type="text"
            placeholder="Логин"
            value={login}
            onChange={(e) => setLogin(e.target.value)}
          />

          <input
            type="password"
            placeholder="Пароль"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />

          <button onClick={loginUser}>Войти</button>

          {captchaRequired && (
            <div>
              <h2>Введите число: {captchaNumber}</h2>

              <input
                type="number"
                placeholder="Повторите число"
                value={captchaInput}
                onChange={(e) => setCaptchaInput(e.target.value)}
              />

              <button onClick={checkCaptcha}>Проверить</button>

              <p>{captchaMessage}</p>
            </div>
          )}

          <button
            onClick={() => {
              setIsRegister(true);
              setMessage("");
            }}
          >
            Регистрация
          </button>
        </>
      )}

      {message && <p>{message}</p>}
    </div>
  );

  //#endregion
}

export default App;
