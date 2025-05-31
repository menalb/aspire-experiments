import { useState } from 'react'
import './App.css'

interface Temperature {
  time: string;
  temperatureCelsius: number;
}

function App() {
  const [text, setText] = useState("")
  const [answer, setAnswer] = useState("")
  const [isAsking, setIsAsking] = useState(false);

  const [temperatures, setWeather] = useState<Array<Temperature>>([]);
  const [weatherLoading, setWeatherLoading] = useState(false);
  const [weatherError, setWeatherError] = useState<string | null>(null);

  const [cityName, setCityName] = useState("");
  const [cityFullName, setCityFullName] = useState("");

  const fetchTemperature = () => {
    setWeatherLoading(true);
    fetch(`/api/weatherforecast?cityName=${cityName}`)
      .then(res => {
        if (!res.ok) throw new Error('Failed to fetch weather');
        return res.json();
      })
      .then(data => {
        setCityFullName(data.cityName);
        setWeather(data.temperatures ?? []);
        setWeatherLoading(false);
      })
      .catch(err => {
        setWeatherError(err.message);
        setWeatherLoading(false);
      });
  };

  const ask = () => {

    const message = `Given these data (time of the day and temperature in celsius): ${JSON.stringify(temperatures)}.
     Please, provide a simple and direct answer, just give the day, without any other detail.
  Question: "${text}"
     `;
    setIsAsking(true);
    setAnswer("");
    fetch('/api/chat', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ question: message }),
    })
      .then(res => res.json())
      .then(data => {
        console.log("Response from ask:", data);
        setAnswer(data);
        setIsAsking(false);
      })
      .catch(err => {
        console.error("Error asking question:", err);
        alert("Failed to get response from server.");
        setIsAsking(false);
      });
  };

  return (
    <>
      <form style={{ margin: '2rem' }}
        onSubmit={(e) => {
          e.preventDefault();
          fetchTemperature();
        }}>
        <div>
          City
          <input style={{ margin: '1rem' }} type='text' onChange={(e) => setCityName(e.target.value)} />
        </div>
        <div>
          <button type='submit'>Submit</button>
        </div>
      </form>
      {weatherLoading && <div>Loading weather...</div>}
      {weatherError && <div style={{ color: 'red' }}>{weatherError}</div>}
      {!weatherLoading && !weatherError && temperatures.length > 0 && (<>
        <div style={{ margin: '1rem' }}>
          <textarea
            aria-multiline="true"
            rows={4} cols={40}
            onChange={(e) => setText(e.target.value)}
            value={text}>

          </textarea>
        </div>
        <div>
          <button onClick={() => ask()} >Ask</button>
          {isAsking && <p>Asking ....</p>}
          <p>
            Answer: {answer ? answer : "No answer yet."}
          </p>
        </div>
      </>)}
      <div style={{ margin: "32px auto", maxWidth: 600 }}>
        {temperatures && cityFullName && <h2>Temperatures in {cityFullName}</h2>}


        {!weatherLoading && !weatherError && temperatures.length > 0 && (
          <table style={{ width: "100%", borderCollapse: "collapse" }}>
            <thead>
              <tr>
                <th key="time-col" style={{ borderBottom: "1px solid #ccc", textAlign: "left", padding: "4px" }}>Time</th>
                <th key="temperature-col" style={{ borderBottom: "1px solid #ccc", textAlign: "left", padding: "4px" }}>Temperature</th>
              </tr>
            </thead>
            <tbody>
              {temperatures.map((item, idx) => (
                <tr key={idx}>
                  <td key={item.time} style={{ borderBottom: "1px solid #eee", padding: "4px" }}>{item.time}</td>
                  <td key={`${item.time}-temperature`} style={{ borderBottom: "1px solid #eee", padding: "4px" }}>{item.temperatureCelsius}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </>
  )
}

export default App
