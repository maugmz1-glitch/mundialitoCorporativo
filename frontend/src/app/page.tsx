export default function Home() {
  return (
    <div>
      <h1>Mundialito Tournament</h1>
      <p>Manage teams, players, matches, and view standings.</p>
      <ul>
        <li><a href="/teams">Teams</a> – CRUD and list with filters</li>
        <li><a href="/players">Players</a> – Register players per team</li>
        <li><a href="/matches">Matches</a> – Schedule and record results</li>
        <li><a href="/standings">Standings</a> – League table and top scorers</li>
      </ul>
    </div>
  );
}
