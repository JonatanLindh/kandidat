using System;
using System.Collections.Generic;
using System.Linq;

public class AliasMethodVose
{
	private readonly Random _random;
	private readonly List<float> _prob;
	private readonly List<int> _alias;


	public AliasMethodVose(IEnumerable<float> probabilities)
		: this(probabilities, new Random())
	{
	}
	
	
	public AliasMethodVose(IEnumerable<float> probabilities, Random random)
	{
		_random = random;
		
		if (probabilities == null)
			throw new ArgumentException("Probabilities cannot be null or empty.", nameof(probabilities));
		
		// Copy the probabilities to a list
		var enumerable = probabilities.ToList();
		
		if (enumerable.Count == 0)
			throw new ArgumentException("Probabilities cannot be null or empty.", nameof(probabilities));
		
		var amount = enumerable.Count;
		
		// Allocate the probability and alias arrays with default values
		_prob = Enumerable.Repeat(0f, amount).ToList();
		_alias = Enumerable.Repeat(0, amount).ToList();
		
		
		var average = 1f / amount;
		
		var small = new Stack<int>();
		var large = new Stack<int>();
		
		for (var i = 0; i < amount; i++)
		{
			if (enumerable[i] < average)
				small.Push(i);
			else
				large.Push(i);
		}
		
		// Now we need to process the small and large stacks.
		while (small.Count > 0 && large.Count > 0)
		{
			var less = small.Pop();
			var more = large.Pop();
			
			//These probabilities have not yet been scaled up to be such that
			// 1/n is given weight 1.0.  We do this here instead.
			_prob[less] = enumerable[less] * amount;
			_alias[less] = more;
			
			// Decrease the probability of the larger one by the appropriate amount.
			enumerable[more] = enumerable[more] + enumerable[less] - average;
			
			// If the new probability is less than the average, push it to the small stack.
			// Otherwise, push it to the large stack.
			if (enumerable[more] < average)
				small.Push(more);
			else
				large.Push(more);
		}
		
		// Now we need to set the probabilities of the remaining items.
		while (small.Count > 0)
			_prob[small.Pop()] = 1f;
		while (large.Count > 0)
			_prob[large.Pop()] = 1f;
		
	}
	
	/// <summary>
	/// Returns a random index from the list of probabilities.
	/// </summary>
	/// <returns>A random value from the underlying distribution</returns>
	public int Next()
	{
		// Generate a random column from 0 to n-1
		int column = _random.Next(_prob.Count); 
		
		// Generate a random coin toss
		bool coinToss = _random.NextDouble() < _prob[column];
		
		// Return the column with a probability of p, or the alias with a probability of 1-p
		return coinToss ? column : _alias[column];
	}
	
}